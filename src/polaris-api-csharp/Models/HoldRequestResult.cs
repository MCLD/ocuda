using System;

namespace Clc.Polaris.Api.Models
{
	/// <summary>
	/// Contains the results of a hold request creation.
	/// </summary>
	public class HoldRequestResult : PapiResponseCommon
	{
		/// <summary>
		/// Hold request GUID.
		/// </summary>
		public Guid RequestGuid { get; set; }

		/// <summary>
		/// TxnGroupQualifier of the hold request.
		/// </summary>
		public string TxnGroupQualifier { get; set; }

		/// <summary>
		/// TxnQualifier of the hold request.
		/// </summary>
		public string TxnQualifier { get; set; }

		/// <summary>
		/// Status type of the hold request.
		/// </summary>
		public int StatusType { get; set; }

		/// <summary>
		/// Status vlue of the hold request.
		/// </summary>
		public int StatusValue { get; set; }

		/// <summary>
		/// Display text.
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// Position of this hold in the queue.
		/// </summary>
		public int QueuePostition { get; set; }

		/// <summary>
		/// Total number of holds in the queue.
		/// </summary>
		public int QueueTotal { get; set; }
	}
}