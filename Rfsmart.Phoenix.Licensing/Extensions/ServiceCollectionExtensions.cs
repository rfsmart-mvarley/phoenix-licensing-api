using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.SSO;
using Amazon.TimestreamQuery;
using Amazon.TimestreamWrite;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Rfsmart.Phoenix.Licensing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rfsmart.Phoenix.Licensing.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAwsServices(
        this IServiceCollection services,
        WebApplicationBuilder builder
    )
        {
            if (builder.Environment.IsDevelopment())
            {
                var sharedFile = new SharedCredentialsFile("C:\\Dev\\.aws\\credentials");
                sharedFile.TryGetProfile("ICS-phxdev_Admin", out var profile);
                AWSCredentialsFactory.TryGetAWSCredentials(profile, sharedFile, out var credentials);

                services
                    .AddSingleton<IAmazonTimestreamWrite>(
                        (s) =>
                        {
                            return new AmazonTimestreamWriteClient(
                                credentials,
                                Amazon.RegionEndpoint.USEast1
                            );
                        }
                    )
                    .AddSingleton<IAmazonTimestreamQuery>(
                        (s) =>
                        {
                            return new AmazonTimestreamQueryClient(
                                credentials,
                                Amazon.RegionEndpoint.USEast1
                            );
                        }
                    );
            }
            else
            {
                services.AddAWSService<IAmazonTimestreamWrite>();
                services.AddAWSService<IAmazonTimestreamQuery>();
            }

            return services;
        }
    }
}
