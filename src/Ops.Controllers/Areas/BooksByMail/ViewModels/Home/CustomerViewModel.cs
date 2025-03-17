using System.Collections.Generic;
using BooksByMail.Data.Models;
using BooksByMail.Models;

namespace BooksByMail.ViewModels.Home
{
    public class CustomerViewModel
    {
        public Customer Customer { get; set; }
        public Comment Comment { get; set; }
        public PolarisPatron Patron { get; set; }
        public List<PolarisItem> PatronCheckouts { get; set; }
        public List<PolarisItem> PatronHolds { get; set; }
        public int PatronHistoryCount { get; set; }
    }
}
