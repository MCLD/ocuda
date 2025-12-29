using System.Collections.Generic;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Controllers.Areas.BooksByMail.ViewModels
{
    public class IndexViewModel : BooksByMailViewModelBase
    {
        public ICollection<CustomerLookup> CustomerLookup { get; set; }
        public int OrderBy { get; set; }
        public bool OrderDesc { get; set; }
        public int? SearchCount { get; set; }
    }
}