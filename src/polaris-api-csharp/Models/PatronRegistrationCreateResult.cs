using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clc.Polaris.Api.Models
{
    /// <summary>
    /// Result of a PatronRegistration call
    /// </summary>
	public class PatronRegistrationCreateResult : PapiResponseCommon
	{
        /// <summary>
        /// Patron's barcode
        /// </summary>
		public string Barcode { get; set; }

        /// <summary>
        /// Patron ID
        /// </summary>
		public int PatronID { get; set; }
	}
}
