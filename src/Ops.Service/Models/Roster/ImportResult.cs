using System.Collections.Generic;
using System.Data;

namespace Ocuda.Ops.Service.Models.Roster
{
    internal class ImportResult
    {
        public ImportResult()
        {
            ReportParameters = new Dictionary<string, string>();
            ReportData = new DataTable();
        }

        public DataTable ReportData { get; }
        public IDictionary<string, string> ReportParameters { get; }
    }
}