using System.Collections.Generic;

namespace Ocuda.Utility.Models
{
    public class ModelStateStorage
    {
        public List<ModelStateItem> ModelStateItems { get; set; }
        public long Time { get; set; }
    }
}
