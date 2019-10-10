namespace Clc.Polaris.Api.Models
{
	/// <summary>
	/// The response to a HoldRequestReply call.
	/// </summary>
	public class HoldRequestReplyData
	{
		/// <summary>
		/// Txn group qualifier.
		/// </summary>
		public string TxnGroupQualifier { get; set; }

		/// <summary>
		/// Txn qualifier.
		/// </summary>
		public string TxnQualifier { get; set; }

		/// <summary>
		/// The org ID of the branch processing the request.
		/// </summary>
		public int RequestingOrgID { get; set; }

		/// <summary>
		/// The answer. 1 - Yes. 0 - No or Cancel
		/// </summary>
		public int Answer { get; set; }

		/// <summary>
		/// Which conditional will this answer be applied to? 
		/// 1 - Item available locally
		/// 2 - Accept ILL policy
		/// 3 - Accept even with existing holds
		/// 4 - No items attached, still place hold
		/// 5 - Accept local hold policy (charge)
		/// </summary>
		public int State { get; set; }
	}
}