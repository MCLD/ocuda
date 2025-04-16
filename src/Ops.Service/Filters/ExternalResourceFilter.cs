using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Filters
{
    public class ExternalResourceFilter : BaseFilter
    {
        public ExternalResourceType? ExternalResourceType { get; set; }

        public ExternalResourceFilter(int? page = null, int take = 15) : base(page, take)
        {
        }
    }
}