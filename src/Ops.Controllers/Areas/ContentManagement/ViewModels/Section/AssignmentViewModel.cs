using System.Collections.Generic;
using System.ComponentModel;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Section
{
    public class AssignmentViewModel
    {
        public AssignmentViewModel()
        {
            Assigned = new Dictionary<int, string>();
            Available = new Dictionary<int, string>();
            InUse = new Dictionary<int, IList<Models.Entities.FileLibrary>>();
        }

        public IDictionary<int, IList<Models.Entities.FileLibrary>> InUse { get; }

        public IDictionary<int, string> Assigned { get; }

        public IDictionary<int, string> Available { get; }

        [DisplayName("Section")]
        public string SectionName { get; set; }

        [DisplayName("Section Slug")]
        public string SectionSlug { get; set; }

        [DisplayName("File Library Slug")]
        public string FileLibrarySlug { get; set; }

        [DisplayName("File Library")]
        public string FileLibraryName { get; set; }

        [DisplayName("File Name")]
        public string FileName { get; set; }

        public int FileId { get; set; }

        public string InUseJson { get; set; }

        public string BaseLink { get; set; }
    }
}
