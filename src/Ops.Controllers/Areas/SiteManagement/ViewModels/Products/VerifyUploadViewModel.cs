using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Products
{
    public class VerifyUploadViewModel : ProductViewModel
    {
        public IDictionary<int, int> Adjustments { get; set; }
        public string AdjustmentsJson { get; set; }
        public bool IsReplenishment { get; set; }
        public IDictionary<string, string> Issues { get; set; }
        public IList<Promenade.Models.Entities.Location> Locations { get; set; }

        public IEnumerable<SelectListItem> LocationSelect
        {
            get
            {
                return Locations.Select(_ => new SelectListItem
                {
                    Text = _.Name,
                    Value = _.Id.ToString(System.Globalization.CultureInfo.InvariantCulture)
                });
            }
        }

        public string UploadType
        {
            get
            {
                return IsReplenishment ? "Replenishment" : "Distribution";
            }
        }

        public int GetNewCount(int locationId, int newValue)
        {
            int oldCount = LocationInventories.SingleOrDefault(_ => _.LocationId == locationId)?.ItemCount ?? 0;
            int adjustment = IsReplenishment ? newValue : newValue * -1;
            return IsReplenishment ? oldCount + newValue : System.Math.Max(0, oldCount - newValue);
        }

        public ProductLocationInventory.Status GetStatus(int locationId)
        {
            return GetStatus(locationId, null);
        }

        public ProductLocationInventory.Status GetStatus(int locationId, int? newValue)
        {
            var li = LocationInventories.SingleOrDefault(_ => _.LocationId == locationId);

            return newValue.HasValue ? li.GetStatus(newValue) : li.InventoryStatus;
        }

        public string GetStatusClass(int locationId)
        {
            return GetStatusClass(locationId, null);
        }

        public string GetStatusClass(int locationId, int? newValue)
        {
            return StatusClass(GetStatus(locationId, newValue));
        }
    }
}