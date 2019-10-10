using System;
using System.Collections.Generic;

namespace Clc.Polaris.Api.Models
{
	/// <summary>
	/// The result of a HoldRequestActivation.
	/// </summary>
	public class HoldRequestActivationResult : PapiResponseCommon
	{
		/// <summary>
		/// Information about activated holds.
		/// </summary>
		public List<HoldActivationRow> HoldActivationRows { get; set; }
	}

	/// <summary>
	/// Information about activated holds.
	/// </summary>
	public class HoldActivationRow
	{
		/// <summary>
		/// The SysHoldRequestID of the hold.
		/// </summary>
		public int SysHoldRequestID { get; set; }

		/// <summary>
		///  0 - Success
		/// -1 - Failure
		/// </summary>
		public int ReturnCode { get; set; }

		/// <summary>
		/// The new activation date of the hold request.
		/// </summary>
		public DateTime NewActivationDate { get; set; }

		/// <summary>
		/// The new expiration date of the hold request.
		/// </summary>
		public DateTime NewExpirationDate { get; set; }

		/// <summary>
		/// The error message if not sucessful.
		/// </summary>
		public string ErrorMessage { get; set; }
	}
}