using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clc.Polaris.Api.Models
{
    /// <summary>
    /// Result of a PatronSearch call
    /// </summary>
	public class PatronSearchResult : PapiResponseCommon
	{
        /// <summary>
        /// List of keywords
        /// </summary>
		public string WordList { get; set; }

        /// <summary>
        /// Total records found
        /// </summary>
		public int TotalRecordsFound { get; set; }

        /// <summary>
        /// Patron search results
        /// </summary>
		public List<PatronSearchRow> PatronSearchRows { get; set; }
	}

    /// <summary>
    /// Patron search result
    /// </summary>
	public class PatronSearchRow
	{
        /// <summary>
        /// Patron ID
        /// </summary>
		public int PatronID { get; set; }

        /// <summary>
        /// Patron barcode
        /// </summary>
		public string Barcode { get; set; }

        /// <summary>
        /// Patron registered branch
        /// </summary>
		public int OrganizationID { get; set; }

        /// <summary>
        /// Patron first and last name
        /// </summary>
		public string PatronFirstLastName { get; set; }
	}
}
