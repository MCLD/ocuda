using System.Collections.Generic;

namespace Ocuda.Utility.Models
{
    public class ModelStateItem
    {
        public string Key { get; set; }
        public string AttemptedValue { get; set; }
        public object RawValue { get; set; }
        public ICollection<string> ErrorMessages { get; set; } = new List<string>();
    }
}
