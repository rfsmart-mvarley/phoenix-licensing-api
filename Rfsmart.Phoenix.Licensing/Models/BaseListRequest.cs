using Rfsmart.Phoenix.Database.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Rfsmart.Phoenix.Licensing.Models
{
    [ExcludeFromCodeCoverage]
    public abstract class BaseListRequest<TOrderBy>
      where TOrderBy : Enum
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public abstract TOrderBy? OrderBy { get; set; }

        /// <summary>
        /// The direction in which the returned values should be ordered.
        /// </summary>
        [DefaultValue(SearchDirection.Descending)]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SearchDirection Direction { get; set; } = SearchDirection.Descending;

        /// <summary>
        /// The number of records that will be returned in a single call.
        /// </summary>
        [DefaultValue(20)]
        [Range(1, 100)]
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// The page number that should be returned. This will be used to calculate how many records to skip.
        /// The calculation will be (PageNumber - 1) * PageSize
        /// </summary>
        [DefaultValue(1)]
        [Range(1, 500)]
        public int PageNumber { get; set; } = 1;

        internal int Skip => (PageNumber - 1) * PageSize;

        public abstract string? Filter { get; set; }
    }

}
