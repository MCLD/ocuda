using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Products
{
    public class IndexViewModel : PaginateModel
    {
        public string BaseLink { get; set; }
        public bool IsSiteManager { get; set; }
        public ICollection<Product> Products { get; set; }

        public string MakeLink(string slug)
        {
            if (!string.IsNullOrEmpty(BaseLink))
            {
                return BaseLink.TrimEnd('/') + "/status/" + slug;
            }
            else
            {
                return null;
            }
        }
    }
}
