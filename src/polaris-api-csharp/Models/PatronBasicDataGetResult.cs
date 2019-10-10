using System;
using System.Collections.Generic;

namespace Clc.Polaris.Api.Models
{
	/// <summary>
	/// The result of a PatronBasicDataGet API call.
	/// </summary>
	public class PatronBasicDataGetResult : PapiResponseCommon
	{
		/// <summary>
		/// Patron information for the supplied patron.
		/// </summary>
		public PatronData PatronBasicData { get; set; }

        public override string ToString()
        {
            if (PatronBasicData?.PatronID == 0) return base.ToString();
            return $"{PatronBasicData.PatronID} - {PatronBasicData.Barcode} - {PatronBasicData.NameFirst} {PatronBasicData.NameLast}";
        }
    }

	/// <summary>
	/// Information about a patron.
	/// </summary>
	public class PatronData
	{
		/// <summary>
		/// The patron's ID.
		/// </summary>
		public int PatronID { get; set; }

		/// <summary>
		/// The patron's barcode.
		/// </summary>
		public string Barcode { get; set; }

		/// <summary>
		/// The patron's first name.
		/// </summary>
		public string NameFirst { get; set; }

		/// <summary>
		/// The patron's last name.
		/// </summary>
		public string NameLast { get; set; }

		/// <summary>
		/// The patron's middle name.
		/// </summary>
		public string NameMiddle { get; set; }

		/// <summary>
		/// The patron's phone number.
		/// </summary>
		public string PhoneNumber { get; set; }

		/// <summary>
		/// The patron's email address.
		/// </summary>
		public string EmailAddress { get; set; }

		/// <summary>
		/// The number of items the patron has out.
		/// </summary>
		public int ItemsOutCount { get; set; }

		/// <summary>
		/// The number of overdue items the patron has out.
		/// </summary>
		public int ItemsOverdueCount { get; set; }

		/// <summary>
		/// The number of lost items the patron has out.
		/// </summary>
		public int ItemsOutLostCount { get; set; }

		/// <summary>
		/// The total number of hold requests the patron has.
		/// </summary>
		public int HoldRequestsTotalCount { get; set; }

		/// <summary>
		/// The number of shipped hold requests the patron has.
		/// </summary>
		public int HoldRequestsShippedCount { get; set; }

		/// <summary>
		/// The number of unclaimed hold requests the patron has.
		/// </summary>
		public int HoldRequestsUnclaimedCount { get; set; }

		/// <summary>
		/// The patron's charge balance.
		/// </summary>
		public decimal ChargeBalance { get; set; }

		/// <summary>
		/// The patron's credit balance.
		/// </summary>
		public decimal CreditBalance { get; set; }

		/// <summary>
		/// The patron's deposit balance.
		/// </summary>
		public decimal DepositBalance { get; set; }

		/// <summary>
		/// The patron's title.
		/// </summary>
		public string NameTitle { get; set; }

		/// <summary>
		/// The patron's suffix.
		/// </summary>
		public string NameSuffix { get; set; }

		/// <summary>
		/// The patron's second phone number.
		/// </summary>
		public string PhoneNumber2 { get; set; }

		/// <summary>
		/// The patron's second phone number.
		/// </summary>
		public string PhoneNumber3 { get; set; }

		/// <summary>
		/// The carrier for the patron's first phone.
		/// </summary>
		public int Phone1CarrierID { get; set; }

		/// <summary>
		/// he carrier for the patron's second phone.
		/// </summary>
		public int Phone2CarrierID { get; set; }

		/// <summary>
		/// he carrier for the patron's third phone.
		/// </summary>
		public int Phone3CarrierID { get; set; }

		/// <summary>
		/// The patron's cell phone number.
		/// </summary>
		public string CellPhone { get; set; }

		/// <summary>
		/// The carrier for the patron's cell phone.
		/// </summary>
		public int CellPhoneCarrierID { get; set; }

		/// <summary>
		/// The patron's alternate email address.
		/// </summary>
		public string AltEmailAddress { get; set; }

		/// <summary>
		/// The patron's birth date.
		/// </summary>
		public DateTime? BirthDate { get; set; }

		/// <summary>
		/// The date the patron registered.
		/// </summary>
		public DateTime? RegistrationDate { get; set; }

		/// <summary>
		/// The date of this patron's last activity.
		/// </summary>
		public DateTime? LastActivityDate { get; set; }

		/// <summary>
		/// The date this patron will require an access check.
		/// </summary>
		public DateTime AddrCheckDate { get; set; }

		/// <summary>
		/// The number of new messages the patron has.
		/// </summary>
		public int MessageNewCount { get; set; }

		/// <summary>
		/// The number of read messages the patron has.
		/// </summary>
		public int MessageReadCount { get; set; }

		/// <summary>
		/// A list of the patron's addresses.
		/// </summary>
		public List<PatronAddress> PatronAddresses { get; set; }

		/// <summary>
		/// The patron's mobile phone number.
		/// </summary>
		public string MobilePhone { get; set; }
	}
}