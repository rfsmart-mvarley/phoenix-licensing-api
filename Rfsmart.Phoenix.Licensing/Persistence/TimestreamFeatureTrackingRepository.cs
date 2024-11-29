using Amazon.TimestreamQuery;
using Amazon.TimestreamQuery.Model;
using Amazon.TimestreamWrite;
using Amazon.TimestreamWrite.Model;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Rfsmart.Phoenix.Common.Context;
using Rfsmart.Phoenix.Common.Models;
using Rfsmart.Phoenix.Licensing.Interfaces;
using Rfsmart.Phoenix.Licensing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using MeasureValueType = Amazon.TimestreamWrite.MeasureValueType;
using ConflictException = Amazon.TimestreamQuery.Model.ConflictException;
using Amazon.Runtime.Internal;

namespace Rfsmart.Phoenix.Licensing.Persistence
{
    public class TimestreamFeatureTrackingRepository : IFeatureTrackingRepository
    {
        private static string s_measure_measureName = "users";
        private static string s_dim_organization = "organization";
        private static string s_dim_tenant = "tenant";
        private static string s_dim_feature = "feature";

        private readonly long _magneticStoreRetentionPeriodInDays = 365;
        private readonly long _memoryStoreRetentionPeriodInDays = 30;
        private static string s_dbName = "licensing";
        private static string s_tableName = "feature-tracking";
        //private static string s_count = "count";
        private static string s_users = "users";
        private readonly IAmazonTimestreamWrite _writeClient;
        private readonly IAmazonTimestreamQuery _queryClient;
        private readonly ILogger<TimestreamFeatureTrackingRepository> _logger;
        private readonly IContextProvider<TenantContext> _contextProvider;

        private static string HOSTNAME = "host-24Gju";

        // See records ingested into this table so far
        private static string SELECT_ALL_QUERY = $"SELECT * FROM {s_dbName}.{s_tableName}";

        //1. Find the average, p90, p95, and p99 CPU utilization for a specific EC2 host over the past 2 hours.
        private static string QUERY_1 = $@"SELECT region, az, hostname, BIN(time, 15s) AS binned_timestamp, 
                                            ROUND(AVG(measure_value::double), 2) AS avg_cpu_utilization, 
                                            ROUND(APPROX_PERCENTILE(measure_value::double, 0.9), 2) AS p90_cpu_utilization, 
                                            ROUND(APPROX_PERCENTILE(measure_value::double, 0.95), 2) AS p95_cpu_utilization, 
                                            ROUND(APPROX_PERCENTILE(measure_value::double, 0.99), 2) AS p99_cpu_utilization 
                                        FROM {s_dbName}.{s_tableName} 
                                        WHERE measure_name = 'cpu_utilization' AND hostname = '{HOSTNAME}' AND time > ago(2h) 
                                        GROUP BY region, hostname, az, BIN(time, 15s) 
                                        ORDER BY binned_timestamp ASC";

        //2. Identify EC2 hosts with CPU utilization that is higher by 10%  or more compared to the average CPU utilization of the entire fleet for the past 2 hours.
        private static string QUERY_2 = $@"WITH avg_fleet_utilization AS (
                                            SELECT COUNT(DISTINCT hostname) AS total_host_count, AVG(measure_value::double) AS fleet_avg_cpu_utilization 
                                            FROM {s_dbName}.{s_tableName} 
                                            WHERE measure_name = 'cpu_utilization' 
                                                AND time > ago(2h) 
                                        ), avg_per_host_cpu AS ( 
                                            SELECT region, az, hostname, AVG(measure_value::double) AS avg_cpu_utilization 
                                            FROM {s_dbName}.{s_tableName} 
                                            WHERE measure_name = 'cpu_utilization' 
                                                AND time > ago(2h) 
                                            GROUP BY region, az, hostname 
                                        ) 
                                        SELECT region, az, hostname, avg_cpu_utilization, fleet_avg_cpu_utilization 
                                        FROM avg_fleet_utilization, avg_per_host_cpu 
                                        WHERE avg_cpu_utilization > 1.1 * fleet_avg_cpu_utilization 
                                        ORDER BY avg_cpu_utilization DESC";

        //3. Find the average CPU utilization binned at 30 second intervals for a specific EC2 host over the past 2 hours.
        private static string QUERY_3 = $@"SELECT BIN(time, 30s) AS binned_timestamp, ROUND(AVG(measure_value::double), 2) AS avg_cpu_utilization, 
                                        hostname FROM {s_dbName}.{s_tableName} 
                                        WHERE measure_name = 'cpu_utilization' 
                                            AND hostname = '{HOSTNAME}' 
                                            AND time > ago(2h) 
                                        GROUP BY hostname, BIN(time, 30s) 
                                        ORDER BY binned_timestamp ASC";

        //4. Find the average CPU utilization binned at 30 second intervals for a specific EC2 host over the past 2 hours, filling in the missing values using linear interpolation.
        private static string QUERY_4 = $@"WITH binned_timeseries AS (
                                            SELECT hostname, BIN(time, 30s) AS binned_timestamp, ROUND(AVG(measure_value::double), 2) AS avg_cpu_utilization 
                                            FROM {s_dbName}.{s_tableName} 
                                            WHERE measure_name = 'cpu_utilization' 
                                               AND hostname = '{HOSTNAME}' 
                                                AND time > ago(2h) 
                                            GROUP BY hostname, BIN(time, 30s) 
                                        ), interpolated_timeseries AS ( 
                                            SELECT hostname, 
                                                INTERPOLATE_LINEAR( 
                                                    CREATE_TIME_SERIES(binned_timestamp, avg_cpu_utilization), 
                                                        SEQUENCE(min(binned_timestamp), max(binned_timestamp), 15s)) AS interpolated_avg_cpu_utilization 
                                            FROM binned_timeseries 
                                            GROUP BY hostname 
                                        ) 
                                        SELECT time, ROUND(value, 2) AS interpolated_cpu 
                                        FROM interpolated_timeseries 
                                        CROSS JOIN UNNEST(interpolated_avg_cpu_utilization)";

        //5. Find the average CPU utilization binned at 30 second intervals for a specific EC2 host over the past 2 hours, filling in the missing values using interpolation based on the last observation carried forward.
        private static string QUERY_5 = $@"WITH binned_timeseries AS ( 
                                            SELECT hostname, BIN(time, 30s) AS binned_timestamp, ROUND(AVG(measure_value::double), 2) AS avg_cpu_utilization 
                                            FROM {s_dbName}.{s_tableName} 
                                            WHERE measure_name = 'cpu_utilization' 
                                                AND hostname = '{HOSTNAME}' 
                                                AND time > ago(2h) 
                                            GROUP BY hostname, BIN(time, 30s) 
                                        ), interpolated_timeseries AS ( 
                                            SELECT hostname, 
                                                INTERPOLATE_LOCF( 
                                                    CREATE_TIME_SERIES(binned_timestamp, avg_cpu_utilization), 
                                                        SEQUENCE(min(binned_timestamp), max(binned_timestamp), 15s)) AS interpolated_avg_cpu_utilization 
                                            FROM binned_timeseries 
                                            GROUP BY hostname 
                                        ) 
                                        SELECT time, ROUND(value, 2) AS interpolated_cpu 
                                        FROM interpolated_timeseries 
                                        CROSS JOIN UNNEST(interpolated_avg_cpu_utilization)";

        //6. Find the average CPU utilization binned at 30 second intervals for a specific EC2 host over the past 2 hours, filling in the missing values using interpolation based on a constant value.
        private static string QUERY_6 = $@"WITH binned_timeseries AS ( 
                                            SELECT hostname, BIN(time, 30s) AS binned_timestamp, ROUND(AVG(measure_value::double), 2) AS avg_cpu_utilization 
                                            FROM {s_dbName}.{s_tableName} 
                                            WHERE measure_name = 'cpu_utilization' 
                                               AND hostname = '{HOSTNAME}' 
                                                AND time > ago(2h) 
                                            GROUP BY hostname, BIN(time, 30s) 
                                        ), interpolated_timeseries AS ( 
                                            SELECT hostname, 
                                                INTERPOLATE_FILL( 
                                                    CREATE_TIME_SERIES(binned_timestamp, avg_cpu_utilization), 
                                                        SEQUENCE(min(binned_timestamp), max(binned_timestamp), 15s), 10.0) AS interpolated_avg_cpu_utilization 
                                            FROM binned_timeseries 
                                            GROUP BY hostname 
                                        ) 
                                        SELECT time, ROUND(value, 2) AS interpolated_cpu 
                                        FROM interpolated_timeseries 
                                        CROSS JOIN UNNEST(interpolated_avg_cpu_utilization)";

        //7. Find the average CPU utilization binned at 30 second intervals for a specific EC2 host over the past 2 hours, filling in the missing values using cubic spline interpolation.
        private static string QUERY_7 = $@"WITH binned_timeseries AS ( 
                                            SELECT hostname, BIN(time, 30s) AS binned_timestamp, ROUND(AVG(measure_value::double), 2) AS avg_cpu_utilization 
                                            FROM {s_dbName}.{s_tableName} 
                                            WHERE measure_name = 'cpu_utilization' 
                                                AND hostname = '{HOSTNAME}' 
                                                AND time > ago(2h) 
                                            GROUP BY hostname, BIN(time, 30s) 
                                        ), interpolated_timeseries AS ( 
                                            SELECT hostname, 
                                                INTERPOLATE_SPLINE_CUBIC( 
                                                    CREATE_TIME_SERIES(binned_timestamp, avg_cpu_utilization), 
                                                        SEQUENCE(min(binned_timestamp), max(binned_timestamp), 15s)) AS interpolated_avg_cpu_utilization 
                                            FROM binned_timeseries 
                                            GROUP BY hostname 
                                        ) 
                                        SELECT time, ROUND(value, 2) AS interpolated_cpu 
                                        FROM interpolated_timeseries 
                                        CROSS JOIN UNNEST(interpolated_avg_cpu_utilization)";

        //8. Find the average CPU utilization binned at 30 second intervals for all EC2 hosts over the past 2 hours, filling in the missing values using linear interpolation.
        private static string QUERY_8 = $@"WITH per_host_min_max_timestamp AS ( 
                                            SELECT hostname, min(time) as min_timestamp, max(time) as max_timestamp 
                                            FROM {s_dbName}.{s_tableName} 
                                            WHERE measure_name = 'cpu_utilization' 
                                                AND time > ago(2h) 
                                            GROUP BY hostname 
                                        ), interpolated_timeseries AS ( 
                                            SELECT m.hostname, 
                                                INTERPOLATE_LOCF( 
                                                    CREATE_TIME_SERIES(time, measure_value::double), 
                                                        SEQUENCE(MIN(ph.min_timestamp), MAX(ph.max_timestamp), 1s)) as interpolated_avg_cpu_utilization 
                                            FROM {s_dbName}.{s_tableName} m 
                                                INNER JOIN per_host_min_max_timestamp ph ON m.hostname = ph.hostname 
                                            WHERE measure_name = 'cpu_utilization' 
                                                AND time > ago(2h) 
                                            GROUP BY m.hostname 
                                        ) 
                                        SELECT hostname, AVG(cpu_utilization) AS avg_cpu_utilization 
                                        FROM interpolated_timeseries 
                                        CROSS JOIN UNNEST(interpolated_avg_cpu_utilization) AS t (time, cpu_utilization) 
                                        GROUP BY hostname 
                                        ORDER BY avg_cpu_utilization DESC";

        //9. Find the percentage of measurements with CPU utilization above 70% for a specific EC2 host over the past 2 hours, filling in the missing values using linear interpolation.
        private static string QUERY_9 = $@"WITH time_series_view AS ( 
                                            SELECT INTERPOLATE_LINEAR( 
                                                CREATE_TIME_SERIES(time, ROUND(measure_value::double,2)), 
                                                SEQUENCE(min(time), max(time), 10s)) AS cpu_utilization 
                                            FROM {s_dbName}.{s_tableName} 
                                            WHERE hostname = '{HOSTNAME}' 
                                                AND  
                                        measure_name = 'cpu_utilization' 
                                                AND time > ago(2h) 
                                            GROUP BY hostname 
                                        ) 
                                        SELECT FILTER(cpu_utilization, x -> x.value > 70.0) AS cpu_above_threshold, 
                                            REDUCE(FILTER(cpu_utilization, x -> x.value > 70.0), 0, (s, x) -> s + 1, s -> s) AS count_cpu_above_threshold, 
                                            ROUND(REDUCE(cpu_utilization, CAST(ROW(0, 0) AS ROW(count_high BIGINT, count_total BIGINT)), 
                                                (s, x) -> CAST(ROW(s.count_high + IF(x.value > 70.0, 1, 0), s.count_total + 1) AS ROW(count_high BIGINT, count_total BIGINT)), 
                                                s -> IF(s.count_total = 0, NULL, CAST(s.count_high AS DOUBLE) / s.count_total)), 4) AS fraction_cpu_above_threshold 
                                        FROM time_series_view";

        //10. List the measurements with CPU utilization lower than 75% for a specific EC2 host over the past 2 hours, filling in the missing values using linear interpolation.
        private static string QUERY_10 = $@"WITH time_series_view AS ( 
                                             SELECT min(time) AS oldest_time, INTERPOLATE_LINEAR( 
                                                 CREATE_TIME_SERIES(time, ROUND(measure_value::double, 2)), 
                                                 SEQUENCE(min(time), max(time), 10s)) AS cpu_utilization 
                                             FROM {s_dbName}.{s_tableName} 
                                             WHERE  
                                          hostname = '{HOSTNAME}' 
                                                 AND  
                                         measure_name = 'cpu_utilization' 
                                                 AND time > ago(2h) 
                                             GROUP BY hostname 
                                         ) 
                                         SELECT FILTER(cpu_utilization, x -> x.value < 75 AND x.time > oldest_time + 1m) 
                                         FROM time_series_view";

        //11. Find the total number of measurements with of CPU utilization of 0% for a specific EC2 host over the past 2 hours, filling in the missing values using linear interpolation.
        private static string QUERY_11 = $@"WITH time_series_view AS ( 
                                             SELECT INTERPOLATE_LINEAR( 
                                                 CREATE_TIME_SERIES(time, ROUND(measure_value::double, 2)), 
                                                 SEQUENCE(min(time), max(time), 10s)) AS cpu_utilization 
                                             FROM {s_dbName}.{s_tableName} 
                                              WHERE  
                                          hostname = '{HOSTNAME}' 
                                                 AND  
                                         measure_name = 'cpu_utilization' 
                                                 AND time > ago(2h) 
                                             GROUP BY hostname 
                                         ) 
                                         SELECT REDUCE(cpu_utilization, 
                                             DOUBLE '0.0', 
                                             (s, x) -> s + 1, 
                                             s -> s) AS count_cpu 
                                         FROM time_series_view";

        //12. Find the average CPU utilization for a specific EC2 host over the past 2 hours, filling in the missing values using linear interpolation.
        private static string QUERY_12() => $@"WITH time_series_view AS ( 
                                             SELECT INTERPOLATE_LINEAR( 
                                                 CREATE_TIME_SERIES(time, ROUND(measure_value::double, 2)), 
                                                 SEQUENCE(min(time), max(time), 10s)) AS cpu_utilization 
                                             FROM {s_dbName}.{s_tableName} 
                                              WHERE  
                                          hostname = '{HOSTNAME}' 
                                              AND  
                                         measure_name = 'cpu_utilization' 
                                                 AND time > ago(2h) 
                                             GROUP BY hostname 
                                         ) 
                                         SELECT REDUCE(cpu_utilization, 
                                             CAST(ROW(0.0, 0) AS ROW(sum DOUBLE, count INTEGER)), 
                                             (s, x) -> CAST(ROW(x.value + s.sum, s.count + 1) AS ROW(sum DOUBLE, count INTEGER)), 
                                              s -> IF(s.count = 0, NULL, s.sum / s.count)) AS avg_cpu 
                                         FROM time_series_view";

        // https://docs.aws.amazon.com/timestream/latest/developerguide/scheduledqueries-patterns-lastpointfromdevice.html
        private static string QUERY_13(string org, string tenant, string feature) => $@"SELECT feature as feature_name, MAX(time) AS created, MAX_BY(gc_pause, time) AS last_measure
FROM ""{s_dbName}"".""{s_tableName}""
WHERE time < from_milliseconds(1636685271872)
    AND measure_name = '{s_measure_measureName}'
    AND {s_dim_organization} = '{org}'
    AND {s_dim_tenant} = '{tenant}'
    AND {s_dim_feature} = '{feature}'
GROUP BY {s_dim_organization}, {s_dim_tenant}, {s_dim_feature}
ORDER BY feature, time DESC";

        private static string MAX_BY_FEATURE(string org, string tenant, string feature) => $@"select feature, max_by(users, time) as users from ""{s_dbName}"".""{s_tableName}""
where organization='{org}' and
tenant='{tenant}' and
feature='{feature}'
group by feature";

        private static string MAX_ACROSS_FEATURES(string org, string tenant) => $@"select feature, max_by(users, time) as users from ""{s_dbName}"".""{s_tableName}""
where organization='{org}' and
tenant='{tenant}'
group by feature";

        private static string BY_USER(string org, string tenant, string feature, string user) => $@"select * from ""{s_dbName}"".""{s_tableName}""
where organization='{org}' and
tenant='{tenant}' and
users like '%{user}%'
order by time desc limit 1";

        private static string OVERAGES_BY_FEATURE(string org, string tenant, string feature, int count) => $@"select * from ""{s_dbName}"".""{s_tableName}""
where organization='{org}' and
tenant='{tenant}' and
feature='{feature}'
count > '{count}'";

        public TimestreamFeatureTrackingRepository(IContextProvider<TenantContext> contextProvider,
            IAmazonTimestreamWrite writeClient,
            IAmazonTimestreamQuery queryClient,
            ILogger<TimestreamFeatureTrackingRepository> logger)
        {
            s_dbName = $"licensing";
            s_tableName = "feature-tracking";

            _logger = logger;
            _writeClient = writeClient;
            _queryClient = queryClient;
            _contextProvider = contextProvider;
        }

        public Task<int> Delete(DeleteFeatureRecordsRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task<FeatureTrackingRecord?> Get(FeatureTrackingByFeatureRequest request)
        {
            var query = MAX_BY_FEATURE(_contextProvider.Context!.Organization!, _contextProvider.Context!.Tenant!, request.FeatureName);

            try
            {
                var resp = await RunQueryAsync(query);

                if (resp != null)
                {
                    var users = ParseQueryResultSingleColumn(resp, s_users);

                    return new FeatureTrackingRecord
                    {
                        FeatureName = request.FeatureName,
                        Users = users!.Split(',').ToArray()
                    };
                }
            }
            catch (Amazon.TimestreamQuery.Model.ValidationException ex)
            {
                if (ex.Message.Contains("Column") && ex.Message.Contains("does not exist"))
                {
                    return null;
                }

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                //await CreateDatabase();

                //var resp = await RunQueryAsync(query);
                throw;
            }

            return null;
        }

        public Task<IEnumerable<FeatureTrackingRecord>> GetAll()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<FeatureTrackingRecord>> GetCurrentConsumption()
        {
            var query = MAX_ACROSS_FEATURES(_contextProvider.Context!.Organization!, _contextProvider.Context!.Tenant!);

            try
            {
                var resp = await RunQueryAsync(query);

                if (resp != null)
                {
                    return ParseQueryResults(resp, x =>
                    {
                        var feature = resp.ColumnInfo.FirstOrDefault(x => x.Name.Equals("feature", StringComparison.InvariantCultureIgnoreCase));
                        var users = resp.ColumnInfo.FirstOrDefault(x => x.Name.Equals("users", StringComparison.InvariantCultureIgnoreCase));

                        var featureName = x.Data[resp.ColumnInfo.IndexOf(feature)].ScalarValue;
                        var usersList = x.Data[resp.ColumnInfo.IndexOf(users)].ScalarValue;

                        return new FeatureTrackingRecord
                        {
                            FeatureName = featureName,
                            Users = usersList.Split(',').ToArray(),
                        };
                    });
                }

                throw new Exception("Response was null");
            }
            catch (Amazon.TimestreamQuery.Model.ValidationException ex)
            {
                if (ex.Message.Contains("Column") && ex.Message.Contains("does not exist"))
                {
                    
                }

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                //await CreateDatabase();

                //var resp = await RunQueryAsync(query);
                throw;
            }
        }

        public async Task<FeatureTrackingRecord> Insert(FeatureTrackingRecord request)
        {
            try
            {
                await WriteRecords(request);
            }
            catch (Exception ex1)
            {
                try
                {
                    if (ex1.Message.Contains("does not exist", StringComparison.InvariantCultureIgnoreCase))
                    {
                        await CreateDatabase();
                    }

                    await WriteRecords(request);
                }
                catch (Exception ex2)
                {
                    throw new AggregateException([ex1, ex2]);
                }
            }

            return request;
        }

        public Task<PagedResult<FeatureTrackingRecord>> List(BaseListRequest<FeatureRecordSort> request)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<FeatureTrackingRecord>> GetOveragesByFeature(string featureName, int max)
        {
            var query = OVERAGES_BY_FEATURE(_contextProvider.Context!.Organization!, 
                _contextProvider.Context!.Tenant!,
                featureName,
                max
            );

            try
            {
                var resp = await RunQueryAsync(query);

                if (resp != null)
                {
                    return ParseQueryResults(resp, x =>
                    {
                        var feature = resp.ColumnInfo.FirstOrDefault(x => x.Name.Equals("feature", StringComparison.InvariantCultureIgnoreCase));
                        var users = resp.ColumnInfo.FirstOrDefault(x => x.Name.Equals("users", StringComparison.InvariantCultureIgnoreCase));

                        var featureName = x.Data[resp.ColumnInfo.IndexOf(feature)].ScalarValue;
                        var usersList = x.Data[resp.ColumnInfo.IndexOf(users)].ScalarValue;

                        return new FeatureTrackingRecord
                        {
                            FeatureName = featureName,
                            Users = usersList.Split(',').ToArray(),
                        };
                    });
                }

                throw new Exception("Response was null");
            }
            catch (Amazon.TimestreamQuery.Model.ValidationException ex)
            {
                if (ex.Message.Contains("Column") && ex.Message.Contains("does not exist"))
                {

                }

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                //await CreateDatabase();

                //var resp = await RunQueryAsync(query);
                throw;
            }
        }

        private async Task CreateDatabase()
        {
            _logger.LogInformation("Creating Database");

            try
            {
                var createDatabaseRequest = new CreateDatabaseRequest
                {
                    DatabaseName = s_dbName,
                };

                var response = await _writeClient.CreateDatabaseAsync(createDatabaseRequest);

                _logger.LogInformation($"Database {s_dbName} created");

                await CreateTable();
            }
            catch (ConflictException)
            {
                _logger.LogError("Database already exists.");

                await CreateTable();
            }
            catch (Exception e)
            {
                _logger.LogError("Create database failed:" + e.ToString());
            }
        }

        private async Task CreateTable()
        {
            _logger.LogInformation("Creating Table");

            try
            {
                var createTableRequest = new CreateTableRequest
                {
                    DatabaseName = s_dbName,
                    TableName = s_tableName,
                    RetentionProperties = new RetentionProperties
                    {
                        MagneticStoreRetentionPeriodInDays = _magneticStoreRetentionPeriodInDays,
                        MemoryStoreRetentionPeriodInHours = _memoryStoreRetentionPeriodInDays * 24,
                    }
                };

                var response = await _writeClient.CreateTableAsync(createTableRequest);

                _logger.LogInformation($"Table {s_tableName} created");
            }
            catch (ConflictException)
            {
                _logger.LogError("Table already exists.");
            }
            catch (Exception e)
            {
                _logger.LogError("Create table failed:" + e.ToString());
            }
        }

        public async Task WriteRecords(FeatureTrackingRecord request)
        {
            _logger.LogInformation("Writing records");

            DateTimeOffset now = DateTimeOffset.UtcNow;
            string currentTimeString = (now.ToUnixTimeMilliseconds()).ToString();

            List<Dimension> dimensions = new List<Dimension> {
                new Dimension { Name = s_dim_organization, Value = _contextProvider.Context!.Organization },
                new Dimension { Name = s_dim_tenant, Value = _contextProvider.Context!.Tenant },
                new Dimension { Name = s_dim_feature, Value = request.FeatureName },
            };

            var userConsumption = new Record
            {
                Dimensions = dimensions,
                MeasureName = "user_metrics",
                MeasureValues = new List<MeasureValue>
                {
                    new MeasureValue
                    {
                        Name = "count",
                        Value = request.UserCount.ToString(),
                        Type = MeasureValueType.DOUBLE,
                    },
                    new MeasureValue
                    {
                        Name = "users",
                        Value = string.Join(',', request.Users),
                        Type = MeasureValueType.VARCHAR,
                    },
                },
                MeasureValueType = MeasureValueType.MULTI,
                Time = currentTimeString,
            };

            List<Record> records = new List<Record> {
                userConsumption,
            };

            try
            {
                var writeRecordsRequest = new WriteRecordsRequest
                {
                    DatabaseName = s_dbName,
                    TableName = s_tableName,
                    Records = records
                };
                WriteRecordsResponse response = await _writeClient.WriteRecordsAsync(writeRecordsRequest);
                _logger.LogInformation($"Write records status code: {response.HttpStatusCode.ToString()}");
            }
            catch (RejectedRecordsException e)
            {
                PrintRejectedRecordsException(e);
            }
            catch (Exception e)
            {
                _logger.LogError("Write records failure:" + e.ToString());
            }
        }

        public void PrintRejectedRecordsException(RejectedRecordsException e)
        {
            _logger.LogWarning("RejectedRecordsException:" + e.Message);
            foreach (RejectedRecord rr in e.RejectedRecords)
            {
                _logger.LogWarning("RecordIndex " + rr.RecordIndex + " : " + rr.Reason);
                long? existingVersion = rr.ExistingVersion;
                if (existingVersion != null)
                {
                    _logger.LogWarning("Rejected record existing version: " + existingVersion);
                }
            }
        }

        public async Task<QueryResponse> RunQueryAsync(string queryString)
        {
            try
            {
                QueryRequest queryRequest = new QueryRequest();
                queryRequest.QueryString = queryString;
                QueryResponse queryResponse = await _queryClient.QueryAsync(queryRequest);
                while (true)
                {
                    ParseQueryResult(queryResponse);
                    if (queryResponse.NextToken == null)
                    {
                        break;
                    }
                    queryRequest.NextToken = queryResponse.NextToken;
                    queryResponse = await _queryClient.QueryAsync(queryRequest);
                }

                return queryResponse;
            }
            catch (Exception e)
            {
                // Some queries might fail with 500 if the result of a sequence function has more than 10000 entries
                _logger.LogError(e.ToString());
                throw;
            }
        }

        private IEnumerable<T> ParseQueryResults<T>(QueryResponse response, Func<Row, T> mapper)
        {
            if (response.Rows.Count <= 0)
            {
                throw new Exception("No rows available to convert!");
            }

            return response.Rows.Select(mapper);
        }

        private string? ParseQueryResultSingleColumn(QueryResponse response, string columnName)
        {
            if (response.Rows.Count <= 0)
            {
                return default;
            }
            else if (response.Rows.Count > 1)
            {
                throw new Exception("Too many rows returned");
            }

            var column = response.ColumnInfo.FirstOrDefault(x => x.Name.Equals(columnName, StringComparison.InvariantCultureIgnoreCase));

            if (column is null)
            {
                throw new Exception("Incorrect column for search");
            }

            var data = response.Rows[0].Data[response.ColumnInfo.IndexOf(column)];

            return data?.ScalarValue;
        }

        private List<string> ParseQueryResult(QueryResponse response)
        {
            List<ColumnInfo> columnInfo = response.ColumnInfo;
            List<string> rowInfo = new List<string>();
            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                //To ignore all null-value properties, see all possible JsonIgnoreCondition values from .net original documentation
            };
            List<String> columnInfoStrings = columnInfo.ConvertAll(x => JsonSerializer.Serialize(x, options));
            List<Row> rows = response.Rows;

            _logger.LogInformation("Metadata:" + string.Join(",", columnInfoStrings));
            _logger.LogInformation("Data:");

            foreach (Row row in rows)
            {
                var parsedRow = ParseRow(columnInfo, row);
                rowInfo.Add(parsedRow);
                _logger.LogInformation(parsedRow);
            }

            return rowInfo;
        }

        private string ParseRow(List<ColumnInfo> columnInfo, Row row)
        {
            List<Datum> data = row.Data;
            List<string> rowOutput = new List<string>();
            for (int j = 0; j < data.Count; j++)
            {
                ColumnInfo info = columnInfo[j];
                Datum datum = data[j];
                rowOutput.Add(ParseDatum(info, datum));
            }
            return $"{{{string.Join(",", rowOutput)}}}";
        }

        private string ParseDatum(ColumnInfo info, Datum datum)
        {
            if (datum.NullValue)
            {
                return $"{info.Name}=NULL";
            }

            Amazon.TimestreamQuery.Model.Type columnType = info.Type;
            if (columnType.TimeSeriesMeasureValueColumnInfo != null)
            {
                return ParseTimeSeries(info, datum);
            }
            else if (columnType.ArrayColumnInfo != null)
            {
                List<Datum> arrayValues = datum.ArrayValue;
                return $"{info.Name}={ParseArray(info.Type.ArrayColumnInfo, arrayValues)}";
            }
            else if (columnType.RowColumnInfo != null && columnType.RowColumnInfo.Count > 0)
            {
                List<ColumnInfo> rowColumnInfo = info.Type.RowColumnInfo;
                Row rowValue = datum.RowValue;
                return ParseRow(rowColumnInfo, rowValue);
            }
            else
            {
                return ParseScalarType(info, datum);
            }
        }

        private string ParseTimeSeries(ColumnInfo info, Datum datum)
        {
            var timeseriesString = datum.TimeSeriesValue
                .Select(value => $"{{time={value.Time}, value={ParseDatum(info.Type.TimeSeriesMeasureValueColumnInfo, value.Value)}}}")
                .Aggregate((current, next) => current + "," + next);

            return $"[{timeseriesString}]";
        }

        private string ParseScalarType(ColumnInfo info, Datum datum)
        {
            return ParseColumnName(info) + datum.ScalarValue;
        }

        private string ParseColumnName(ColumnInfo info)
        {
            return info.Name == null ? "" : (info.Name + "=");
        }

        private string ParseArray(ColumnInfo arrayColumnInfo, List<Datum> arrayValues)
        {
            return $"[{arrayValues.Select(value => ParseDatum(arrayColumnInfo, value)).Aggregate((current, next) => current + "," + next)}]";
        }


    }
}
