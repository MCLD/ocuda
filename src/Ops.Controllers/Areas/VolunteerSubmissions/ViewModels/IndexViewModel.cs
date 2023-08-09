using System.Collections.Generic;
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
        public ICollection<VolunteerFormSubmission> Submissions { get; }
        public IDictionary<string, int> SubmissionTypes { get; }
    }
}