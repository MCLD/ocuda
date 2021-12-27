using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Products
{
    public class MappingViewModel
    {
        public IEnumerable<LocationProductMap> LocationMap { get; set; }
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

        public Product Product { get; set; }
    }
}