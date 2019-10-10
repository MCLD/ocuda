using System;
using System.Collections.Generic;
using System.Text;

namespace Ocuda.Ops.Models
{
    public class PolarisItem
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string Category { get; set; }
        public string HoldStatus { get; set; }
        public DateTime CheckoutDate { get; set; }
        public DateTime DueDate { get; set; }
    }
}
