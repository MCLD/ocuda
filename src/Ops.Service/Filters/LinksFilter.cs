using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Filters
{
    public class LinksFilter(int? page = null, int take = 15)
        : BaseFilter(page, take)
    {
        public LinkLibrary LinkLibrary { get; set; }

        public bool OnlyCount { get; set; }
    }
}
