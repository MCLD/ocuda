namespace Ocuda.Ops.Service.Models.Roster
{
    internal class ImportFieldDefinition
    {
        public ImportFieldDefinition(string fieldTitle)
        {
            FieldTitle = fieldTitle;
        }

        public int? FieldPosition { get; set; }
        public string FieldTitle { get; }
        public string? Section { get; set; }
    }
}