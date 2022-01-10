using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Products
{
    public class ProductViewModel
    {
        public ICollection<ProductLocationInventory> LocationInventories { get; set; }
        public Product Product { get; set; }

        public static string StatusClass(ProductLocationInventory.Status status) =>
            status switch
            {
                ProductLocationInventory.Status.None => "table-danger",
                ProductLocationInventory.Status.Few => "table-warning",
                ProductLocationInventory.Status.Many => "table-success",
                _ => null
            };

        public static string StatusTextClass(ProductLocationInventory.Status status) =>
            status switch
            {
                ProductLocationInventory.Status.None => "text-danger",
                ProductLocationInventory.Status.Few => "text-warning",
                ProductLocationInventory.Status.Many => "text-success",
                _ => null
            };
    }
}
