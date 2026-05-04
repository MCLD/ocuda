using System.Collections.Generic;

namespace Ocuda.Models
{
    public class AddressAssociation
    {
        public IEnumerable<string> Entities { get; set; } = default!;
        public string PostalCode { get; set; } = default!;
        public string PropertyType { get; set; } = default!;
        public string StreetAddress1 { get; set; } = default!;
    }
}