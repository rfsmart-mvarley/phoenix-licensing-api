using System;

namespace Rfsmart.Phoenix.Licensing.Models
{
    public class DeleteFeatureRecordsRequest
    {
        public required DateTime From { get; set; }
        public required DateTime To { get; set; }
    }

}
