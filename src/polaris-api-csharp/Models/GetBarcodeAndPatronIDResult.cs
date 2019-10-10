using System.Collections.Generic;

namespace Clc.Polaris.Api.Models
{
	/// <summary>
	/// Contains the result of a Patron_GetBarcodeFromID request.
	/// </summary>
	public class GetBarcodeAndPatronIDResult : PapiResponseCommon
	{
		/// <summary>
		/// A collection of rows containing a PatronID and barcode.
		/// </summary>
		public List<BarcodeAndPatronIDRow> BarcodeAndPatronIDRows { get; set; }
	}

	/// <summary>
	/// A simple row that contains a PatronID and barcode.
	/// </summary>
	public class BarcodeAndPatronIDRow
	{
		/// <summary>
		/// The patron's ID
		/// </summary>
		public int PatronID { get; set; }

		/// <summary>
		/// The patron's barcode
		/// </summary>
		public string Barcode { get; set; }
	}
}