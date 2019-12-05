using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    public class Section : Abstract.BaseEntity
    {
        public string Name { get; set; }

        public string EmbedVideoUrl { get; set; }

        public string Icon { get; set; }

        [Required]
        public string Stub { get; set; }
    }
}
