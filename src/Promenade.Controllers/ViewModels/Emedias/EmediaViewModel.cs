using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers.ViewModels.Emedias
{
    public class EmediaViewModel
    {
        public ICollection<EmediaGroup> GroupedEmedia { get; set; }
    }
}
