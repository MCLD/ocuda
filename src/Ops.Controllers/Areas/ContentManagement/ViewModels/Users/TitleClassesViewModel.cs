using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Users
{
    public class TitleClassesViewModel : PaginateModel
    {
        public TitleClassesViewModel()
        {
            TitleClasses = new List<TitleClass>();
            Titles = new List<string>();
        }

        public List<TitleClass> TitleClasses { get; }
        public List<string> Titles { get; }
    }
}
