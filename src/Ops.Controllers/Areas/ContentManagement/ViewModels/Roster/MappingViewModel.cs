using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Ops.Models.Interfaces;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Roster
{
    public class MappingViewModel
    {
        public int ClearId { get; set; }
        public bool IsClear { get; set; }
        public bool IsDivision { get; set; }
        public IDictionary<int, string> Locations { get; set; }

        public IEnumerable<SelectListItem> LocationsSelectList
        {
            get
            {
                return Locations != null
                    ? new SelectList(Locations, "Key", "Value")
                    : new SelectList(new List<string>());
            }
        }

        public IEnumerable<IRosterLocationMapping> Mapping { get; set; }
        public int SelectedLocation { get; set; }
        public string Summary { get; set; }
        public int UpdateId { get; set; }

        public string GetLocation(int? locationId)
        {
            if (locationId.HasValue
                && Locations?.ContainsKey(locationId.Value) == true)
            {
                return Locations[locationId.Value];
            }
            return "Unknown location";
        }
    }
}