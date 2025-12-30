namespace Ocuda.Models
{
    public class CustomerAddress
    {
        public string AddressType { get; set; }
        public int AddressTypeId { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public int CountryId { get; set; }
        public string County { get; set; }
        public int Id { get; set; }
        public string PostalCode { get; set; }
        public string State { get; set; }
        public string StreetAddressOne { get; set; }
        public string StreetAddressTwo { get; set; }
        public string ZipPlusFour { get; set; }
    }
}