using Microsoft.AspNetCore.Authorization;
using Rfsmart.Phoenix.Api.Auth;
using Rfsmart.Phoenix.Licensing.Auth;
using System.Diagnostics.CodeAnalysis;

namespace Rfsmart.Phoenix.Licensing.Web.Auth
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    [ExcludeFromCodeCoverage]
    public static class AuthorizationOptionExtensions
    {
        public static AuthorizationOptions AddLicensingPolicies(
            this AuthorizationOptions options,
            string issuer
        )
        {
            options
                .AddPermissionsPolicy(
                    Policies.LicenseWrite,
                    issuer,
                    [Permissions.LicenseWrite, Permissions.LicenseRead]
                )
                .AddOrgScopePolicy(Policies.OrganizationLicenseWrite)
                .AddTenantScopePolicy(Policies.TenantLicenseWrite);

            options
                .AddPermissionsPolicy(Policies.LicenseRead, issuer, [Permissions.LicenseRead])
                .AddOrgScopePolicy(Policies.OrganizationLicenseRead)
                .AddTenantScopePolicy(Policies.TenantLicenseRead);

            return options;
        }
    }
}
