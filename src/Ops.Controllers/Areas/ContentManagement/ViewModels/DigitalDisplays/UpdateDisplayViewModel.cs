using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.DigitalDisplays
{
    public class UpdateDisplayViewModel
    {
        public DigitalDisplay DigitalDisplay { get; set; }
        public IEnumerable<SelectListItem> DigitalDisplaySets { get; set; }
        public IEnumerable<SelectListItem> Locations { get; set; }
    }
}