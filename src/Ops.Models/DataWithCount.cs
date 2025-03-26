using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ocuda.Ops.Models
{
    public class DataWithCount<DataType>
    {
        public DataType Data { get; set; }
        public int Count { get; set; }
    }
}
