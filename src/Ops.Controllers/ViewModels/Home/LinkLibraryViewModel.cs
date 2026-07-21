using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.ViewModels.Home
{
    public class LinkLibraryViewModel : PaginateModel
    {
        public LinkLibraryViewModel()
        {
            Links = [];
            FileTypes = [];
        }

        public bool IsSiteManager { get; set; }

        public bool HasAdminRights { get; set; }

        [Required]
        public string SectionSlug { get; set; }

        [Required]
        public string SectionName { get; set; }

        [Required]
        [DisplayName("LinkLibrary Slug")]
        public string LinkLibrarySlug { get; set; }

        [Required]
        [DisplayName("LinkLibrary Name")]
        public string LinkLibraryName { get; set; }

        public int LinkLibraryId { get; set; }

        public LinkLibrary LinkLibrary { get; set; }

        public Link Link { get; set; }

        public ICollection<Link> Links { get; }

        public ICollection<FileType> FileTypes { get; }
    }
}
