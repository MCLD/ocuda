using System.Collections.Generic;
using System.ComponentModel;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Section
{
    public class FileLibraryPermissionsViewModel
    {
        public FileLibraryPermissionsViewModel()
        {
            AssignedGroups = new Dictionary<int, string>();
            AvailableGroups = new Dictionary<int, string>();
        }

        public IDictionary<int, string> AssignedGroups { get; }

        public IDictionary<int, string> AvailableGroups { get; }

        [DisplayName("Stub")]
        public string FileLibraryStub { get; set; }

        [DisplayName("File Library")]
        public string Name { get; set; }

        [DisplayName("Section")]
        public string SectionName { get; set; }

        public string SectionStub { get; set; }
    }
}