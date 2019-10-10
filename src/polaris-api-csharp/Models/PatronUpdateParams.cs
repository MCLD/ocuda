using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clc.Polaris.Api.Models
{
	/// <summary>
	/// The parameters required to make a PatronUpdate request.
	/// </summary>
	public class PatronUpdateParams
	{
        /// <summary>
        /// Transaction branch ID
        /// </summary>
        public int BranchId { get; set; } = 1;

        /// <summary>
        /// Transaction user ID
        /// </summary>
        public int UserId { get; set; } = 1;

        /// <summary>
        /// Transaction workstation ID
        /// </summary>
        public int WorkstationId { get; set; } = 1;

        /// <summary>
        /// Enable or Disable the reading list feature for the patron.
        /// </summary>
        public bool? ReadingListEnabled { get; set; }

		/// <summary>
		/// The format the patron wishes to receive email in. 1 = Plain Text, 2 = HTML
		/// </summary>
		public int? EmailFormat { get; set; }

		/// <summary>
		/// The method the patron wishes to use to receive their notifications.
		/// 1- Mailing address
		/// 2- Email address
		/// 3- Telephone 1
		/// 4- Telephone 2
		/// 5- Telephone 3
		/// 6- FAX
		/// 7 - EDI
		/// 8- TXT Messaging
		/// </summary>
		public int? DeliveryOptionID { get; set; }

        /// <summary>
        /// The patron's email address.
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
		/// The patron's alternate email address.
		/// </summary>
		public string AltEmailAddress { get; set; }

        /// <summary>
        /// The patron's phone number.
        /// </summary>
        public string PhoneVoice1 { get; set; }
        public string PhoneVoice2 { get; set; }
        public string PhoneVoice3 { get; set; }

        public int? Phone1CarrierID { get; set; }
        public int? Phone2CarrierID { get; set; }
        public int? Phone3CarrierID { get; set; }

        public int? TxtPhoneNumber { get; set; }

        /// <summary>
        /// Enable 'Additional Txt notice'
        /// </summary>
        public bool? EnableSMS { get; set; }

        /// <summary>
        /// The patron's new Password/PIN
        /// </summary>
        public string NewPassword { get; set; }

        public DateTime? AddressCheckDate { get; set; }
        public DateTime? ExpirationDate { get; set; }


        public List<PatronAddress> Addresses { get; set; } = new List<PatronAddress>();
	}
}
