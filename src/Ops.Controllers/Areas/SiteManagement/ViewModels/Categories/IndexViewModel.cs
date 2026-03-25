using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Categories
{
    public class IndexViewModel : PaginateModel
    {
        public IndexViewModel()
        {
            Categories = [];
            Subjects = [];
        }

        public ICollection<Category> Categories { get; }
        public Category Category { get; set; }
        public Subject Subject { get; set; }
        public ICollection<Subject> Subjects { get; }
    }
}