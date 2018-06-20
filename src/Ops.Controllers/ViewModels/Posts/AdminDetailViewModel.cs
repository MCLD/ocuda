using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Controllers.ViewModels.Posts
{
    public class AdminDetailViewModel
    {
        public Post Post { get; set; }
        public string Action { get; set; }
    }
}
