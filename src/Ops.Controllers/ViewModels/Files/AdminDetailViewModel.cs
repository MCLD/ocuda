using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Controllers.ViewModels.Files
{
    public class AdminDetailViewModel
    {
        public SectionFile SectionFile { get; set; }
        public string Action { get; set; }
        public IFormFile File { get; set; }
    }
}
