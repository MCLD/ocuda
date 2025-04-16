namespace Ocuda.Ops.Models.Entities
{
    public class SectionCategory
    {
        public int SectionId { get; set; }
        public Section Section { get; set; }

        public int CategoryId { get; set; }

        public Category Category { get; set; }
    }
}