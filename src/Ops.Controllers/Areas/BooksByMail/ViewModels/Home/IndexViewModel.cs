using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ocuda.Ops.Models;

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
