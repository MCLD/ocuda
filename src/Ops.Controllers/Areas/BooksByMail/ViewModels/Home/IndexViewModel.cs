using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BooksByMail.Models;

namespace BooksByMail.ViewModels.Home
{
    public class IndexViewModel
    {
        public ICollection<PolarisPatron> Patrons { get; set; }
        public PaginateModel PaginateModel { get; set; }
        public int OrderBy { get; set; }
        public bool OrderDesc { get; set; }
        public string Search { get; set; }
        public int? SearchCount { get; set; }
    }
}
