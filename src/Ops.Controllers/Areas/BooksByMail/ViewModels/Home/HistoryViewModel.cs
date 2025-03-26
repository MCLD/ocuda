using System.Collections.Generic;
using BooksByMail.Models;

namespace Ocuda.Ops.Controllers.Areas.BooksByMail.ViewModels.Home
{
    public class HistoryViewModel
    {
        public ICollection<Material> Items { get; set; }
        public PaginateModel PaginateModel { get; set; }
        public int OrderBy { get; set; }
        public bool OrderDesc { get; set; }
    }
}
