using System.Collections.Generic;

namespace Ocuda.Models
{
    public class AddressLookupResult
    {
        public string Error { get; set; }
        public IEnumerable<string> Residents { get; set; }
        public string PostalCode { get; set; }
        public string StreetAddress1 { get; set; }
        public bool Success { get; set; }
    }
}
