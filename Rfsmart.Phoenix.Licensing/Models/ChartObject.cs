using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Rfsmart.Phoenix.Licensing.Models
{
    public enum ChartType
    {
        line,
        bar
    }

    public class ChartObject
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required ChartType Type { get; set; }

        public required ChartData Data { get; set; }
    }

    public class ChartData
    {
        public required string[] Labels { get; set; }
        [JsonPropertyName("datasets")]
        public required ChartDataSet[] DataSets { get; set; } 
        public required ChartOptions Options { get; set; }
    }

    public class ChartDataSet
    {
        public required string Label { get; set; }
        public required string BackgroundColor { get; set; }
        public required string BorderColor { get; set; }
        public required int[] Data { get; set; }
        public bool Fill { get; set; }
    }

    public class ChartOptions
    {
        public required ChartTitleOption Title { get; set; }
    }

    public class ChartTitleOption
    {
        public required string Text { get; set; }
        public bool Display { get; set; } = true;
    }
}
