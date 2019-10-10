using System;

namespace Clc.Polaris.Api.Models
{
	/// <summary>
	/// The parameters required to perform a NotificationUpdate.
	/// </summary>
	public class NotificationUpdateParams
	{
        /// <summary>
        /// Branch where the notification is being updates
        /// </summary>
        public int BranchId { get; set; } = 1;

        /// <summary>
        /// User updating the notification
        /// </summary>
        public int UserId { get; set; } = 1;

        /// <summary>
        /// Workstation the notification is being updated on
        /// </summary>
        public int WorkstationId { get; set; } = 1;



        /// <summary>
        /// The DeliveryOptionID of the notification. Currently only phone notifications are supported; DeliveryOptionIDs 3, 4, and 5.
        /// </summary>
        public int DeliveryOptionId { get; set; }

		/// <summary>
		/// How the message was delivered. In the currently implementation this is the patron's phone number.
		/// </summary>
		public string DeliveryString { get; set; }

        /// <summary>
        /// The date the notice was delivered
        /// </summary>
        public DateTime DeliveryDate { get; set; } = DateTime.Now;

		/// <summary>
		/// The ID of the patron.
		/// </summary>
		public int PatronId { get; set; }

		/// <summary>
		/// The ID of the item record on the notification.
		/// </summary>
		public int ItemRecordId { get; set; }

		/// <summary>
		/// Any additional data/notes.
		/// </summary>
		public string Details { get; set; }

		/// <summary>
		/// The status of the notification.
		/// </summary>
		public NotificationStatus NotificationStatus { get; set; }

		/// <summary>
		/// The type of notification it was.
		/// </summary>
		public int NotificationTypeId { get; set; }

        public int ReportingOrgID { get; set; } = 1;
	}
}