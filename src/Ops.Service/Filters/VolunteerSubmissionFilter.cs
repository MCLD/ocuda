using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Filters
{
    public class VolunteerSubmissionFilter : BaseFilter
    {
        public VolunteerSubmissionFilter(int page) : base(page)
        {
        }

        public VolunteerFormType? FormType { get; set; }
        public int LocationId { get; set; }
        public int SelectedLocation { get; set; }
    }
}