using System;

namespace Ocuda.Ops.Models
{
    public class Customer
    {
        public int PatronID { get; set; }
        public string Barcode { get; set; }
        public string NameFirst { get; set; }
        public string NameLast { get; set; }
        public DateTime? LastActivityDate { get; set; }
        public string LastActivityClass { get; set; }
    }
}