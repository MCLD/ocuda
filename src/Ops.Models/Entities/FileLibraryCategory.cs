namespace Ocuda.Ops.Models.Entities
{
    public class FileLibraryCategory
    {
        public int FileLibraryId { get; set; }

        public FileLibrary FileLibrary { get; set; }

        public int CategoryId { get; set; }

        public Category Category { get; set; }
    }
}
