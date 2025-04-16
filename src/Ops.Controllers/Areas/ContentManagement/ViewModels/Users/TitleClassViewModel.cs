using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Users
{
    public class TitleClassViewModel
    {
        public TitleClassViewModel()
        {
            Titles = new List<string>();
        }

        public TitleClass TitleClass { get; set; }
        public List<string> Titles { get; set; }
    }
}