using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Products
{
    public class ProductViewModel
    {
        public Product Product { get; set; }

        public ICollection<ProductLocationInventory> LocationInventories { get; set; }
    }
}
