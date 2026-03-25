using System.Collections.Generic;

namespace Ocuda.Models
{
    /// <summary>
    /// This enum supports the <see cref="ESource"/> DTO for importing data from external systems.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
        "CA1008:Enums should have zero value",
        Justification = "Zero is not a valid value for an import")]
    public enum ESourceAccessLevel
    {
        Disabled = 1,
        LoginRequired = 2,
        NoLoginRequired = 3
    }

    /// <summary>
    /// This is a DTO for handling imports of ESource data exported from external systems.
    /// </summary>
    public class ESourceImport
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage",
            "CA2227:Collection properties should be read only",
            Justification = "Data import may need to replace this object")]
        public ICollection<string> Categories { get; set; }

        public string? Description { get; set; }
        public int ESourceTargetID { get; set; }
        public ESourceAccessLevel InHouseAccess { get; set; }
        public bool IsHttpPost { get; set; }
        public string Link { get; set; } = null!;
        public string? Message { get; set; }
        public string Name { get; set; } = null!;
        public ESourceAccessLevel RemoteAccess { get; set; }
    }
}