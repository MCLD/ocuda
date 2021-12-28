using System.Collections.Generic;

namespace Ocuda.Utility.Models
{
    public class ICollectionWithCount<TItem>
    {
        public int Count { get; set; }
        public ICollection<TItem> Data { get; set; }
    }
}
