using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BooksByMail.Models
{
    public class PolarisPatron
    {
        public int PatronID { get; set; }
        public string Barcode { get; set; }
        public string NameFirst { get; set; }
        public string NameLast { get; set; }
        public DateTime? LastActivityDate { get; set; }
        public string LastActivityClass { get; set; }
    }
}
