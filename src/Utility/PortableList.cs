using System.Collections.Generic;
using Ocuda.Utility.Abstract;

namespace Ocuda.Utility
{
    public class PortableList<T> : BasePortable
    {
        public IEnumerable<T> Items { get; set; }
    }
}