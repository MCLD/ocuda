using System.Collections.Generic;
using Ocuda.Ops.Models;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.BooksByMail.ViewModels.Home
{
    public class IndexViewModel
    {
        public ICollection<Customer> Patrons { get; set; }
        public PaginateModel PaginateModel { get; set; }
        public int OrderBy { get; set; }
        public bool OrderDesc { get; set; }
        public string Search { get; set; }
        public int? SearchCount { get; set; }
    }
}