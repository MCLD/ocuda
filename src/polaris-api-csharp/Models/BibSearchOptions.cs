using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clc.Polaris.Api.Models.Models
{
    /// <summary>
    /// Options available when performing a bibliographic record search
    /// </summary>
    public class BibSearchOptions
    {
        /// <summary>
        /// Search term
        /// </summary>
        public string Term { get; set; }

        /// <summary>
        /// Search type, keyword or boolean
        /// </summary>
        public BibSearchTypes SearchType { get; set; } = BibSearchTypes.keyword;

        /// <summary>
        /// Sort method of results
        /// </summary>
        public SearchSortOptions SortOption { get; set; } = SearchSortOptions.MP;

        /// <summary>
        /// Which field to search
        /// </summary>
        public SearchQualifiers Qualifier { get; set; } = SearchQualifiers.KW;

        /// <summary>
        /// Limit by filter you want to apply
        /// </summary>
        public string Limit { get; set; }

        /// <summary>
        /// Branch to search
        /// </summary>
        public int Branch { get; set; } = 1;

        /// <summary>
        /// Page
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// Page size
        /// </summary>
        public int PageSize { get; set; } = 10;
    }
}
