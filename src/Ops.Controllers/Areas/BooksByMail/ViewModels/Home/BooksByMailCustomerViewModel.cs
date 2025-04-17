using System.Collections.Generic;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.BooksByMail.ViewModels.Home
{
    public class BooksByMailCustomerViewModel
    {
        public BooksByMailCustomer BooksByMailCustomer { get; set; }
        public BooksByMailComment BooksByMailComment { get; set; }
        public Customer CustomerLookup { get; set; }
        public List<Material> CustomerLookupCheckouts { get; set; }
        public List<Material> CustomerLookupHolds { get; set; }
        public string Search { get; set; }
        public int CustomerLookupHistoryCount { get; set; }
    }
}