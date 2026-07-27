using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Filters
{
    public class FilesFilter(int? page = null, int take = 15)
        : BaseFilter(page, take)
    {
        public FileLibrary FileLibrary { get; set; }

        public bool OnlyCount { get; set; }
    }
}
