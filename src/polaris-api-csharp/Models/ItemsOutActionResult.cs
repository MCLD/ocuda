using System;
using System.Collections.Generic;

namespace Clc.Polaris.Api.Models
{
	/// <summary>
	/// The result of an item renewal.
	/// </summary>
	public class ItemsOutActionResult : PapiResponseCommon
	{
		/// <summary>
		/// The result of the item renewal.
		/// </summary>
		public ItemRenewResult ItemRenewResult { get; set; }
	}

	/// <summary>
	/// Lists of items that could and couldn't be renewed.
	/// </summary>
	public class ItemRenewResult
	{
		/// <summary>
		/// A list of items that could not be renewed.
		/// </summary>
		public List<ItemRenewBlockRow> BlockRows { get; set; }

		/// <summary>
		/// A list of successfully renewed items.
		/// </summary>
		public List<ItemRenewDueDateRow> DueDateRows { get; set; }
	}

	/// <summary>
	/// An item that could not be renewed by the Polaris API.
	/// </summary>
	public class ItemRenewBlockRow
	{
		/// <summary>
		/// A PAPI code to tell which 'process' caused the error
		/// 1 - Patron block
		/// 2 - Item renewal block
		/// </summary>
		public int PAPIErrorType { get; set; }

		/// <summary>
		/// The current error code return via Polaris base processing
		/// </summary>
		public int PolarisErrorCode { get; set; }

		/// <summary>
		/// Is the error overridable.
		/// </summary>
		public bool ErrorAllowOverride { get; set; }

		/// <summary>
		/// Description of the error.
		/// </summary>
		public string ErrorDesc { get; set; }

		/// <summary>
		/// ID of the item record.
		/// </summary>
		public int ItemRecordID { get; set; }
	}

	/// <summary>
	/// An item that was successfully renewed by the Polaris API.
	/// </summary>
	public class ItemRenewDueDateRow
	{
		/// <summary>
		/// ID of the item record.
		/// </summary>
		public int ItemRecordID { get; set; }

		/// <summary>
		/// Due date of the item.
		/// </summary>
		public DateTime DueDate { get; set; }
	}
}