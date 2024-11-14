using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rfsmart.Phoenix.Common.Context;
using Microsoft.AspNetCore.Http;
using Rfsmart.Phoenix.Licensing.Attributes;

namespace Rfsmart.Phoenix.Licensing.Web.Middleware
{
    [ExcludeFromCodeCoverage]
    public class TenantContextValidationMiddleware(
        RequestDelegate _next,
        ILogger<TenantContextValidationMiddleware> _logger
    )
    {
        public async Task InvokeAsync(HttpContext context)
        {
            var tenantContext = context.RequestServices.GetService<
                IContextProvider<TenantContext>
            >();
            if (
                RequiresValidation(context)
                && (
                    string.IsNullOrEmpty(tenantContext?.Context?.Organization)
                    || string.IsNullOrEmpty(tenantContext.Context?.Tenant)
                )
            )
            {
                _logger.LogWarning(
                    $"Header {nameof(TenantContext)} is required and cannot be null."
                );
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync(
                    $"'Tenant' and 'Organization' are required in {nameof(TenantContext)} header and cannot be null. Tenant: {tenantContext?.Context?.Tenant}, Organization: {tenantContext?.Context?.Organization}"
                );
                return;
            }
            await _next(context);
        }

        private static bool RequiresValidation(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint != null)
            {
                // Check if the SkipValidation attribute is applied
                var skipValidation =
                    endpoint.Metadata.GetMetadata<ValidateTenantContextAttribute>();
                return skipValidation != null;
            }
            return false;
        }
    }
}
