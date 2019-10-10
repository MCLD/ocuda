using System;
using System.Collections.Generic;

namespace Clc.Polaris.Api.Models
{
	/// <summary>
	/// Information about the hold requests of a patron.
	/// </summary>
	public class PatronHoldRequestsGetResult : PapiResponseCommon
	{
		/// <summary>
		/// A list of rows containing information about a hold.
		/// </summary>
		public List<PatronHoldRequestsGetRow> PatronHoldRequestsGetRows { get; set; }
	}

	/// <summary>
	/// Information about a hold a patron has placed.
	/// </summary>
	public class PatronHoldRequestsGetRow
	{
		/// <summary>
		/// The ID of the hold request.
		/// </summary>
		public int HoldRequestID { get; set; }

		/// <summary>
		/// The ID of the bibliographic record on hold.
		/// </summary>
		public int BibID { get; set; }

		/// <summary>
		/// The status ID of the hold.
		/// </summary>
		public int StatusID { get; set; }

		/// <summary>
		/// The status description of the hold.
		/// </summary>
		public string StatusDescription { get; set; }

		/// <summary>
		/// Title of the item on hold.
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Author of the item on hold.
		/// </summary>
		public string Author { get; set; }

		/// <summary>
		/// Call number of the item on hold.
		/// </summary>
		public string CallNumber { get; set; }

		/// <summary>
		/// Format ID of the item on hold.
		/// </summary>
		public int FormatID { get; set; }

		/// <summary>
		/// Format description of the item on hold.
		/// </summary>
		public string FormatDescription { get; set; }

		/// <summary>
		/// ID of the branch the hold will be picked up at.
		/// </summary>
		public int PickupBranchID { get; set; }

		/// <summary>
		/// Name of the branch the hold will be picked up at.
		/// </summary>
		public string PickupBranchName { get; set; }

		/// <summary>
		/// Date the item can be picked up by.
		/// </summary>
		public DateTime? PickupByDate { get; set; }

		/// <summary>
		/// Position of this hold in the hold queue.
		/// </summary>
		public int QueuePosition { get; set; }

		/// <summary>
		/// Total number of holds in the hold queue.
		/// </summary>
		public int QueueTotal { get; set; }

		/// <summary>
		/// Date this hold will become active.
		/// </summary>
		public DateTime ActivationDate { get; set; }

		/// <summary>
		/// Date this hold expires.
		/// </summary>
		public DateTime ExpirationDate { get; set; }

		/// <summary>
		/// Name used to identify a group of titles that can satisfy this hold request
		/// </summary>
		public string GroupName { get; set; }

		/// <summary>
		/// Is the hold an item level hold.
		/// </summary>
		public bool ItemLevelHold { get; set; }

		/// <summary>
		/// Is a borrow by mail request.
		/// </summary>
		public bool IsBorrowByMail { get; set; }
	}
}