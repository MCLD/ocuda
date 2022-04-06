using System.Collections.Generic;
using System.ComponentModel;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Products
{
    public class PermissionsViewModel
    {
        public PermissionsViewModel()
        {
            AvailableGroups = new Dictionary<int, string>();
            AssignedGroups = new Dictionary<int, string>();
        }

        public IDictionary<int, string> AssignedGroups { get; }

        public IDictionary<int, string> AvailableGroups { get; }

        public int ProductId { get; set; }

        [DisplayName("Product name")]
        public string ProductName { get; set; }

        public string Slug { get; set; }
    }
}
