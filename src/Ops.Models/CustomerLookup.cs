using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ocuda.Ops.Models
{
    public class CustomerLookup
    {
        public int CustomerLookupID { get; set; }
        public string Barcode { get; set; }
        public string NameFirst { get; set; }
        public string NameLast { get; set; }
        public DateTime? LastActivityDate { get; set; }
        public string LastActivityClass { get; set; }
    }
}
