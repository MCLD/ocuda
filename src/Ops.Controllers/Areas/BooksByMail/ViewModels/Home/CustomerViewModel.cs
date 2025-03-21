using System.Collections.Generic;
using BooksByMail.Data.Models;
using BooksByMail.Models;

namespace BooksByMail.ViewModels.Home
{
    public class BooksByMailCustomerViewModel
    {
        public BooksByMailCustomer BooksByMailCustomer { get; set; }
        public BooksByMailComment BooksByMailComment { get; set; }
        public Customer Patron { get; set; }
        public List<Material> PatronCheckouts { get; set; }
        public List<Material> PatronHolds { get; set; }
        public int PatronHistoryCount { get; set; }
    }
}
