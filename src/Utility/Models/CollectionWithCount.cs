using System.Collections.Generic;

namespace Ocuda.Utility.Models
{
    public class CollectionWithCount<TItem>
    {
        public int Count { get; set; }
        public ICollection<TItem> Data { get; set; }
    }
}