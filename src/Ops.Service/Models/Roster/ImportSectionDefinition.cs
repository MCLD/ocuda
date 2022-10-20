namespace Ocuda.Ops.Service.Models.Roster
{
    internal class ImportSectionDefinition
    {
        public ImportSectionDefinition(string sectionName)
        {
            SectionName = sectionName;
        }

        public string SectionName { get; }
        public int? SectionStartingField { get; set; }
    }
}