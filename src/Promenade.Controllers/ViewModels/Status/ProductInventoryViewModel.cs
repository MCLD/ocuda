using System;
using System.Collections.Generic;

namespace Ocuda.Promenade.Controllers.ViewModels.Status
{
    public class LocationInventory
    {
        public string CurrentStatus { get; set; }
        public string CurrentStatusClass { get; set; }
        public string InventoryStatus { get; set; }
        public string InventoryStatusClass { get; set; }
        public string Name { get; set; }
        public string Stub { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ProductInventoryViewModel
    {
        public ProductInventoryViewModel()
        {
            LocationInventories = new List<LocationInventory>();
        }

        public List<LocationInventory> LocationInventories { get; }
        public string SegmentHeader { get; set; }
        public string SegmentText { get; set; }
        public string Title { get; set; }
    }
}
