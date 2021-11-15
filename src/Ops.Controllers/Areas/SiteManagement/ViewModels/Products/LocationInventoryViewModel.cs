using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Products
{
    public class LocationInventoryViewModel
    {
        public ProductLocationInventory.Status InventoryStatus { get; set; }
        public int LocationId { get; set; }
        public ProductLocationInventory LocationInventory { get; set; }
        public string LocationName { get; set; }
        public Product Product { get; set; }
        public int ProductId { get; set; }
        public string ProductSlug { get; set; }
    }
}