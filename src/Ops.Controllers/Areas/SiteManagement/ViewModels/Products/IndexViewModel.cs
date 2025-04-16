using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Products
{
    public class IndexViewModel : PaginateModel
    {
        public IndexViewModel(ICollection<Product> products, ICollection<string> permissionIds)
        {
            PermissionIds = permissionIds;
            Products = products;
        }

        public IndexViewModel()
        {
            PermissionIds = new List<string>();
            Products = new List<Product>();
        }

        public string BaseLink { get; set; }
        public bool IsSiteManager { get; set; }
        public ICollection<string> PermissionIds { get; }
        public ICollection<Product> Products { get; }

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