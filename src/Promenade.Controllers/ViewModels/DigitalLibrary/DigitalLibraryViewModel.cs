using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers.ViewModels.DigitalLibrary
{
    public class DigitalLibraryViewModel
    {
        public SocialCard SocialCard { get; set; }
        public ICollection<EmediaGroup> GroupedEmedia { get; set; }
    }
}
