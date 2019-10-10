using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clc.Polaris.Api.Models
{
    /// <summary>
    /// Options when creating a new patron registration
    /// </summary>
	public class PatronRegistrationParams
	{
        /// <summary>
        /// Branch processing the registration
        /// </summary>
		public int LogonBranchID { get; set; } = 1;

        /// <summary>
        /// User processing the registration
        /// </summary>
		public int LogonUserID { get; set; } = 1;

        /// <summary>
        /// Workstation processing the registration
        /// </summary>
		public int LogonWorkstationID { get; set; } = 1;

        /// <summary>
        /// Patron's registered branch
        /// </summary>
		public int PatronBranchID { get; set; }

        /// <summary>
        /// ZIP code
        /// </summary>
		public string PostalCode { get; set; }

        /// <summary>
        /// ZIP+4
        /// </summary>
		public int? ZipPlusFour { get; set; }

        /// <summary>
        /// City
        /// </summary>
		public string City { get; set; }

        /// <summary>
        /// State
        /// </summary>
		public string State { get; set; }

        /// <summary>
        /// County
        /// </summary>
		public string County { get; set; }

        /// <summary>
        /// Country ID
        /// </summary>
		public int? CountryID { get; set; }

        /// <summary>
        /// Street one
        /// </summary>
		public string StreetOne { get; set; }

        /// <summary>
        /// Street two
        /// </summary>
		public string StreetTwo { get; set; }

        /// <summary>
        /// First name
        /// </summary>
		public string NameFirst { get; set; }

        /// <summary>
        /// Last name
        /// </summary>
		public string NameLast { get; set; }

        /// <summary>
        /// Middle name
        /// </summary>
		public string NameMiddle { get; set; }

        /// <summary>
        /// User defined field 1
        /// </summary>
		public string User1 { get; set; }

        /// <summary>
        /// User defined field 2
        /// </summary>
		public string User2 { get; set; }

        /// <summary>
        /// User defined field 3
        /// </summary>
		public string User3 { get; set; }

        /// <summary>
        /// User defined field 4
        /// </summary>
		public string User4 { get; set; }

        /// <summary>
        /// User defined field 5
        /// </summary>
		public string User5 { get; set; }

        /// <summary>
        /// Gender
        /// </summary>
		public string Gender { get; set; }

        /// <summary>
        /// Date of birth
        /// </summary>
		public DateTime? Birthdate { get; set; }

        /// <summary>
        /// Phone 1
        /// </summary>
		public string PhoneVoice1 { get; set; }

        /// <summary>
        /// Phone 2
        /// </summary>
		public string PhoneVoice2 { get; set; }

        /// <summary>
        /// Email address
        /// </summary>
		public string EmailAddress { get; set; }

        /// <summary>
        /// Language ID
        /// </summary>
		public int? LanguageID { get; set; }

        /// <summary>
        /// Delivery option ID
        /// </summary>
		public int? DeliveryOptionID { get; set; }

        /// <summary>
        /// Username
        /// </summary>
		public string Username { get; set; }

        /// <summary>
        /// PIN/Password
        /// </summary>
		public string Password { get; set; }

        /// <summary>
        /// Password confirmation
        /// </summary>
		public string Password2 { get; set; }

        /// <summary>
        /// Alternate email address
        /// </summary>
		public string AltEmailAddress { get; set; }

        /// <summary>
        /// Phone 3
        /// </summary>
		public string PhoneVoice3 { get; set; }

        /// <summary>
        /// PHone 1 mobile carrier id
        /// </summary>
		public int? Phone1CarrierID { get; set; }

        /// <summary>
        /// PHone 2 mobile carrier id
        /// </summary>
		public int? Phone2CarrierID { get; set; }

        /// <summary>
        /// PHone 3 mobile carrier id
        /// </summary>
		public int? Phone3CarrierID { get; set; }

        /// <summary>
        /// Enabled SMS notifications
        /// </summary>
		public bool? EnableSMS { get; set; }

        /// <summary>
        /// Which phone (1,2,3) should be used for txt 
        /// </summary>
		public int? TxtPhoneNumber { get; set; }

        /// <summary>
        /// Barcode
        /// </summary>
		public string Barcode { get; set; }

        /// <summary>
        /// eReceipt option ID
        /// </summary>
		public int? EReceiptOptionID { get; set; }

        /// <summary>
        /// PatronCodeID
        /// </summary>
        public int? PatronCode { get; set; }

        /// <summary>
        /// Date the account with expire
        /// </summary>
        public DateTime? ExpirationDate { get; set; }

        /// <summary>
        /// Date the account will require an address check
        /// </summary>
        public DateTime? AddrCheckDate { get; set; }
    }
}
