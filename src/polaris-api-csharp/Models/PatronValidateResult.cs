using System;

namespace Clc.Polaris.Api.Models
{
	/// <summary>
	/// Contains the results of a patron validation request.
	/// </summary>
	public class PatronValidateResult : PapiResponseCommon
	{
		/// <summary>
		/// The patron's barcode.
		/// </summary>
		public string Barcode { get; set; }

		/// <summary>
		/// Indicates valid patron.
		/// </summary>
		public bool ValidPatron { get; set; }

		/// <summary>
		/// The patron's ID.
		/// </summary>
		public int PatronID { get; set; }

		/// <summary>
		/// The patron's code ID.
		/// </summary>
		public int PatronCodeID { get; set; }

		/// <summary>
		/// The patron's assigned branch ID.
		/// </summary>
		public int AssignedBranchID { get; set; }

		/// <summary>
		/// The name of the patron's assigned branch.
		/// </summary>
		public string AssignedBranchName { get; set; }

		/// <summary>
		/// The date this patron's registration expires.
		/// </summary>
		public DateTime? ExpirationDate { get; set; }

		/// <summary>
		/// Indicates if the override was used in place of the patron's password.
		/// </summary>
		public bool OverridePasswordUsed { get; set; }
	}
}