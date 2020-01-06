using Ocuda.Promenade.Models.Abstract;

namespace Ocuda.Promenade.Models.Entities
{
    public class SegmentText : BaseEntity
    {
        public int SegmentId { get; set; }

        public string Header { get; set; }

        public string Text { get; set; }

        public int LanguageId { get; set; }
    }
}
