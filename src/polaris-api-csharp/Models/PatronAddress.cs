namespace Clc.Polaris.Api.Models
{
	/// <summary>
	/// Patron address information.
	/// </summary>
	public struct PatronAddress
	{
		/// <summary>
		/// The ID of the address.
		/// </summary>
		public int AddressId { get; set; }

		/// <summary>
		/// The free text label associated with the address.
		/// </summary>
		public string FreeTextLabel { get; set; }

		/// <summary>
		/// The first street of the address.
		/// </summary>
		public string StreetOne { get; set; }

		/// <summary>
		/// The second street of the address.
		/// </summary>
		public string StreetTwo { get; set; }

		/// <summary>
		/// The city of the address.
		/// </summary>
		public string City { get; set; }

		/// <summary>
		/// The state of the address.
		/// </summary>
		public string State { get; set; }

		/// <summary>
		/// The county the address is in.
		/// </summary>
		public string County { get; set; }

		/// <summary>
		/// The ZIP code of the address.
		/// </summary>
		public string PostalCode { get; set; }

		/// <summary>
		/// The country of the address.
		/// </summary>
		public string Country { get; set; }

		/// <summary>
		/// The ID number of this address' country.
		/// </summary>
		public int CountryID { get; set; }

		/// <summary>
		/// The address type of this address.
		/// </summary>
		public int AddressTypeID { get; set; }


        public string ZipPlusFour { get; set; }
    }
}