using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.VolunteerSubmissions.ViewModels
{
    public class IndexViewModel : ViewModelBase
    {
        public IndexViewModel()
        {
            AllLocationNames = new Dictionary<int, string>();
            Submissions = new List<VolunteerFormSubmission>();
            SubmissionTypes = new Dictionary<string, int>();
        }

        public IDictionary<int, string> AllLocationNames { get; }

        public SelectList LocationNamesDropdown
        {
            get
            {
                return new SelectList(AllLocationNames, "Key", "Value", SelectedLocation);
            }
        }

        public int SelectedLocation { get; set; }
        public ICollection<VolunteerFormSubmission> Submissions { get; }
        public IDictionary<string, int> SubmissionTypes { get; }
    }
}