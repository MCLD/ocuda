using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Filters
{
    public class ExternalResourceFilter : BaseFilter
    {
        public ExternalResourceType? ExternalResourceType { get; set; }

        public ExternalResourceFilter(int? page = null, int take = 15) : base(page, take) { }
    }
}
