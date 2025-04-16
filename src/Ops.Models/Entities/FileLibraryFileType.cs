namespace Ocuda.Ops.Models.Entities
{
    public class FileLibraryFileType
    {
        public int FileLibraryId { get; set; }
        public FileLibrary FileLibrary { get; set; }

        public int FileTypeId { get; set; }
        public FileType FileType { get; set; }
    }
}