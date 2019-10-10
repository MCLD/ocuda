using System.Collections.Generic;

namespace Clc.Polaris.Api.Models
{
	/// <summary>
	/// The reply to a HoldRequestCancelAll call.
	/// </summary>
	public class HoldRequestCancelAllResult : PapiResponseCommon
	{
		/// <summary>
		/// Information about cancelled holds.
		/// </summary>
		public List<HoldRequestCancelRow> HoldRequestCancelRows { get; set; }
	}

	/// <summary>
	/// The information about an individual cancelled hold request.
	/// </summary>
	public class HoldRequestCancelRow
	{
		/// <summary>
		/// The ID of the hold request.
		/// </summary>
		public int SysHoldRequestID { get; set; }

		/// <summary>
		/// The code returned by the Polaris API.
		/// </summary>
		public int ReturnCode { get; set; }

		/// <summary>
		/// The error message returned by the Polaris API.
		/// </summary>
		public string ErrorMessage { get; set; }
	}
}