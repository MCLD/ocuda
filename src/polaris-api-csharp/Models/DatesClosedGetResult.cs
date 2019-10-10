using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clc.Polaris.Api.Models
{
    public class DatesClosedGetResult
    {
        public List<DatesClosedRow> DatesClosedRows { get; set; }

        public override string ToString()
        {
            return string.Join(", ", DatesClosedRows.Select(d => d.ToString()));
        }
    }

    public class DatesClosedRow
    {
        public DateTime DateClosed { get; set; }

        public override string ToString()
        {
            return DateClosed.ToShortDateString();
        }
    }
}
