using System.Collections.Generic;
using System.ComponentModel;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Files
{
    public class IndexViewModel
    {
        public IEnumerable<FileLibrary> Libraries { get; set; }
        public FileLibrary FileLibrary { get; set; }
        public PaginateModel PaginateModel { get; set; }
        public int SectionId { get; set; }
        public ICollection<FileType> FileTypes { get; set; }

        [DisplayName("File Types")]
        public ICollection<int> FileTypeIds { get; set; }
    }
}
