using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Controllers.ViewModels.Home
{
    public class IndexViewModel
    {
        public IEnumerable<Post> Posts { get; set; }
        public IEnumerable<Link> Links { get; set; }
        public IEnumerable<File> Files { get; set; }
        public IEnumerable<Calendar> Calendars { get; set; }
    }
}
