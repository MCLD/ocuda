using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Location
{
    public class LocationFormsViewModel : LocationPartialViewModel
    {
        public string Action { get; set; }
        public List<VolunteerFormSubmission> FormSubmissions { get; set; }
        public IEnumerable<SelectListItem> FormTypes { get; set; }
        public int LocationId { get; set; }

        [DisplayName("Form Type")]
        public int? TypeId { get; set; }
    }
}