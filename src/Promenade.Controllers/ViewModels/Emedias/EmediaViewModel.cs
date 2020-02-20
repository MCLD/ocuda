using Ocuda.Promenade.Models.Entities;
using System.Collections.Generic;

namespace Ocuda.Promenade.Controllers.ViewModels.Emedias
{
    public class EmediaViewModel
    {
        public ICollection<EmediaGroup> GroupedEmedia { get; set; }
    }
}
