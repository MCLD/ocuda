using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers.ViewModels.Emedias
{
    public class EmediaViewModel
    {
        public SocialCard SocialCard { get; set; }
        public ICollection<EmediaGroup> GroupedEmedia { get; set; }
    }
}
