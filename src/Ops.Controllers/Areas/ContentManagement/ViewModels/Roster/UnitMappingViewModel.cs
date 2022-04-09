using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Roster
{
    public class UnitMappingViewModel : PaginateModel
    {
        public IEnumerable<SelectListItem> Locations { get; set; }
        public ICollection<UnitLocationMap> UnitLocationMaps { get; set; }
        public string GetLocationById(int locationId)
        {
            return Locations
                .SingleOrDefault(_ => _.Value == locationId.ToString(CultureInfo.InvariantCulture))?
                .Text;
        }
    }
}
