namespace Ocuda.Ops.Models.Entities
{
    public class FileCategory
    {
        public int FileId { get; set; }

        public File File { get; set; }

        public int CategoryId { get; set; }

        public Category Category { get; set; }
    }
}
