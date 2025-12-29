using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Controllers.ViewModels.Profile
{
    public class IndexViewModel
    {
        public IndexViewModel()
        {
            DirectReports = [];
            Permissions = [];
            RelatedTitleClassifications = new Dictionary<TitleClass, ICollection<User>>();
        }

        public DateTime AuthenticatedAt { get; set; }
        public string AuthenticationInformation { get; set; }
        public bool CanEdit { get; set; }
        public bool CanUpdatePicture { get; set; }
        public bool CanViewLastSeen { get; set; }
        public ICollection<User> DirectReports { get; }
        public IEnumerable<SelectListItem> Locations { get; set; }
        public IList<string> Permissions { get; }
        public string PicturePath { get; set; }
        public IDictionary<TitleClass, ICollection<User>> RelatedTitleClassifications { get; }
        public User User { get; set; }
        public bool UserViewingSelf { get; set; }

        public string GetLocationById(int locationId)
        {
            return Locations
                .SingleOrDefault(_ => _.Value == locationId.ToString(CultureInfo.InvariantCulture))?
                .Text;
        }
    }
}