using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Rfsmart.Phoenix.Licensing.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rfsmart.Phoenix.Licensing.Swagger.Filters
{
    /// <summary>
    /// Operation filter to add the tenant as a required custom header
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class TenantHeaderFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (FiltersHelper.IsTenantContextUsed(context))
            {
                if (operation.Parameters == null)
                {
                    operation.Parameters = [];
                }

                operation.Parameters.Add(
                    new OpenApiParameter
                    {
                        Name = "tenant",
                        In = ParameterLocation.Header,
                        Required = true,
                        Schema = new OpenApiSchema
                        {
                            Type = "string",
                            Default = new OpenApiString("dev"),
                        },
                    }
                );
            }
        }
    }

    /// <summary>
    /// Operation filter to add the tenant as a required custom header
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class OrganizationHeaderFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (FiltersHelper.IsTenantContextUsed(context))
            {
                if (operation.Parameters == null)
                {
                    operation.Parameters = [];
                }

                operation.Parameters.Add(
                    new OpenApiParameter
                    {
                        Name = "organization",
                        In = ParameterLocation.Header,
                        Required = true,
                        Schema = new OpenApiSchema
                        {
                            Type = "string",
                            Default = new OpenApiString("rfsmart"),
                        },
                    }
                );
            }
        }
    }

    [ExcludeFromCodeCoverage]
    internal static class FiltersHelper
    {
        public static bool IsTenantContextUsed(OperationFilterContext context)
        {
            var tenantContextUsed = context
                ?.MethodInfo?.DeclaringType?.GetCustomAttributes(true)
                .OfType<ValidateTenantContextAttribute>()
                .Any();
            return tenantContextUsed ?? false;
        }
    }
}
