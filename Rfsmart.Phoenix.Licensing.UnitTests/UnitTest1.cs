using Rfsmart.Phoenix.Licensing.Helpers;
using Rfsmart.Phoenix.Licensing.Models;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rfsmart.Phoenix.Licensing.UnitTests;

public class Tests
{
    
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        var trackedRecords = DataGenerator.Generate(1000, 15);

        //var result = JsonSerializer.Serialize(trackedRecords.OrderBy(x => x.Created));

        var features = trackedRecords.Select(x => x.FeatureName).Distinct();
        WriteCsv(trackedRecords.OrderBy(x => x.Created), "C:\\Dev\\testdata-postgres-ingest.csv");
        WriteCsvForExcelCharts(trackedRecords.OrderBy(x => x.Created), "C:\\Dev\\testdata-excel-chart.csv", features.ToArray());
        WriteCsvForTimestreamIngestion(trackedRecords.OrderBy(x => x.Created), "C:\\Dev\\testdata-timestream-ingest.csv");
    }

    public static void WriteCsv<T>(IOrderedEnumerable<T> items, string path)
    {
        Type itemType = typeof(T);
        var props = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Where(pi => pi.Name != "UserCount")
                            .ToArray();

        using (var writer = new StreamWriter(path))
        {
            // Writing header (property names)
            writer.WriteLine(string.Join(",", props.Select(p => p.Name)));

            // Writing data
            foreach (var item in items)
            {
                writer.WriteLine(string.Join(",", props.Select(p => GetValue(p, item!))));
            }
        }
    }

    private static string GetValue(PropertyInfo propertyInfo, object obj)
    {
        if (propertyInfo.PropertyType.IsArray)
        {
            var users = propertyInfo.GetValue(obj) as IEnumerable<string>;

            return $"\"{{{string.Join(',', users!)}}}\"";
        }

        if (propertyInfo.PropertyType == typeof(DateTimeOffset))
        {
            var val = propertyInfo.GetValue(obj, null) as DateTimeOffset?;

            return val!.Value.ToString("o", CultureInfo.InvariantCulture)!;
        }

        var value = propertyInfo.GetValue(obj, null);
        return value?.ToString()!.Replace(",", ";") ?? "";  // Escape commas in data
    }

    public static void WriteCsvForExcelCharts(IOrderedEnumerable<FeatureTrackingRecord> items, string path, string[] headers)
    {
        using (var writer = new StreamWriter(path))
        {
            var columns = new List<string>(["", ..headers]);

            writer.WriteLine(string.Join(",", columns));

            // empty,Receiving,Advanced Receiving,Picking
            var processed = new List<FeatureTrackingRecord>();

            // Writing data
            foreach (var item in items)
            {
                string[] columnValues = columns.Select((col, index) =>
                {
                    if (index == 0)
                    {
                        return item.Created.ToString("o", CultureInfo.InvariantCulture);
                    }

                    if (col == item.FeatureName)
                    {
                        return item.UserCount.ToString();
                    }

                    var last = processed.LastOrDefault(x => x.FeatureName == item.FeatureName);

                    if (last != null)
                    {
                        return last.UserCount.ToString();
                    }

                    return "0";
                }).ToArray();

                processed.Add(item);
                writer.WriteLine(string.Join(",", columnValues));
            }
        }
    }

    public static void WriteCsvForTimestreamIngestion(IOrderedEnumerable<FeatureTrackingRecord> items, string path)
    {
        // time, feature, organization, tenant, measure_name, count, users
        // 
        using (var writer = new StreamWriter(path))
        {
            var columns = new List<string>(["time","feature","organization","tenant","measure_name","count","users"]);

            writer.WriteLine(string.Join(",", columns));

            // empty,Receiving,Advanced Receiving,Picking
            var processed = new List<FeatureTrackingRecord>();

            // Writing data
            foreach (var item in items)
            {
                string[] columnValues = columns.Select((col, index) =>
                {
                    switch (col)
                    {
                        case "time":
                            return item.Created.ToUnixTimeMilliseconds().ToString();

                        case "feature":
                            return item.FeatureName;

                        case "organization":
                            return "rfsmart";

                        case "tenant":
                            return "dev";

                        case "measure_name":
                            return "user_metrics";

                        case "count":
                            return item.UserCount.ToString();

                        case "users":
                            return $"\"{string.Join(',', item.Users)}\"";

                        default:
                            return "";
                    }
                }).ToArray();

                processed.Add(item);
                writer.WriteLine(string.Join(",", columnValues));
            }
        }
    }
}