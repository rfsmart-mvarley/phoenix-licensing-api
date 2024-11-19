using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rfsmart.Phoenix.Licensing.Auth
{
    public static class Policies
    {
        public const string LicenseRead = nameof(LicenseRead);
        public const string LicenseWrite = nameof(LicenseWrite);
        public const string OrganizationLicenseRead = nameof(OrganizationLicenseRead);
        public const string OrganizationLicenseWrite = nameof(OrganizationLicenseWrite);
        public const string TenantLicenseRead = nameof(TenantLicenseRead);
        public const string TenantLicenseWrite = nameof(TenantLicenseWrite);
    }
}
