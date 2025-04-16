using System.Collections.Generic;
using Ocuda.Ops.Models;
using Ocuda.Utility.Models;

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