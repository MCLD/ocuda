using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    public class ExternalResource : Abstract.BaseEntity
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        [MaxLength(255)]
        public string Url { get; set; }

        public int SortOrder { get; set; }
        public ExternalResourceType Type { get; set; }
    }

    public enum ExternalResourceType
    {
        CSS,
        JS
    }
}
