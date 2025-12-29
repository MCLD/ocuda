using System.Collections.Generic;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Controllers.Areas.BooksByMail.ViewModels
{
    public class HistoryViewModel : BooksByMailViewModelBase
    {
        public ICollection<Material> Items { get; set; }
        public int OrderBy { get; set; }
        public bool OrderDesc { get; set; }
    }
}