using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clc.Polaris.Api.Models
{
    public partial class RecordSetRecordsGetResult : PapiResponseCommon
    {
        public List<RecordSetRecordsGetRow> RecordSetRecordsGetRows { get; set; }

        public IEnumerable<int> Ids { get { return RecordSetRecordsGetRows.Select(r => r.PatronID); } }

        public override string ToString()
        {
            return string.Join(",", RecordSetRecordsGetRows.OrderBy(r => r.PatronID).Select(r => r.PatronID.ToString()));
        }
    }

    public partial class RecordSetRecordsGetRow
    {
        public int PatronID { get; set; }

        public override string ToString()
        {
            return PatronID.ToString();
        }
    }
}