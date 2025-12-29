using System.Collections.Generic;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.BooksByMail.ViewModels
{
    public class BooksByMailCustomerViewModel : BooksByMailViewModelBase
    {
        public BooksByMailComment BooksByMailComment { get; set; }
        public BooksByMailCustomer BooksByMailCustomer { get; set; }
        public CustomerLookup CustomerLookup { get; set; }
        public IList<Material> CustomerLookupCheckouts { get; set; }
        public int CustomerLookupHistoryCount { get; set; }
        public IList<Material> CustomerLookupHolds { get; set; }
    }
}