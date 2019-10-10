using System;
using System.Collections.Generic;
using System.Text;

namespace Ocuda.Ops.Models
{
    public class DataWithCount<DataType>
    {
        public DataType Data { get; set; }
        public int Count { get; set; }
    }
}
