using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rfsmart.Phoenix.Licensing.Models
{
    public class FeaturesRequest
    {
        public required string[] Features { get; set; }
        public required string User { get; set; }
    }
}
