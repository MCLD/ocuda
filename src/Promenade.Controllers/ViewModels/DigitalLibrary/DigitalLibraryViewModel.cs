using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers.ViewModels.DigitalLibrary
{
    public class DigitalLibraryViewModel
    {
        public DigitalLibraryViewModel()
        {
            GroupedEmedia = [];
            SlugsSubjects = [];
        }

        public string ActiveKey { get; set; }
        public string ButtonAll { get; set; }
        public string ButtonGrouped { get; set; }
        public ICollection<EmediaGroup> GroupedEmedia { get; }
        public bool IsLocalNetwork { get; set; }
        public Dictionary<string, string> SlugsSubjects { get; }
        public SocialCard SocialCard { get; set; }

        public string IsActive(string slugKey)
        {
            return slugKey?.Equals(ActiveKey, System.StringComparison.OrdinalIgnoreCase) == true
                ? "active"
                : null;
        }
    }
}