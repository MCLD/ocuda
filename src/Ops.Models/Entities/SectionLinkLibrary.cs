namespace Ocuda.Ops.Models.Entities
{
    public class SectionLinkLibrary
    {
        public int SectionId { get; set; }

        public Section Section { get; set; }

        public int LinkLibraryId { get; set; }

        public LinkLibrary LinkLibrary { get; set; }
    }
}
