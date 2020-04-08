using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Section
{
    public class LinkLibraryViewModel
    {
        public PaginateModel PaginateModel { get; set; }

        [Required]
        public string SectionStub { get; set; }

        [Required]
        public string SectionName { get; set; }

        [Required]
        [DisplayName("LinkLibrary Stub")]
        public string LinkLibraryStub { get; set; }

        [Required]
        [DisplayName("LinkLibrary Name")]
        public string LinkLibraryName { get; set; }

        public int LinkLibraryId { get; set; }

        public LinkLibrary LinkLibrary { get; set; }

        public Link Link { get; set; }

        public ICollection<Link> Links { get; set; }

        public ICollection<FileType> FileTypes { get; set; }
    }
}
