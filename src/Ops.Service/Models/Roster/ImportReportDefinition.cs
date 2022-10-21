using System.Collections.Generic;

namespace Ocuda.Ops.Service.Models.Roster
{
    internal class ImportReportDefinition
    {
        public ImportReportDefinition(string reportName, int sectionLine, int headerLine)
        {
            Fields = new List<ImportFieldDefinition>();
            HeaderLine = headerLine;
            ReportName = reportName;
            SectionLine = sectionLine;
            Sections = new List<ImportSectionDefinition>();
        }

        public List<ImportFieldDefinition> Fields { get; }
        public int HeaderLine { get; }
        public string ReportName { get; }
        public int SectionLine { get; }
        public List<ImportSectionDefinition> Sections { get; }
    }
}