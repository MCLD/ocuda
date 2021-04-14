using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    public class DigitalDisplayAsset : Abstract.BaseEntity
    {
        [MaxLength(32)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance",
            "CA1819:Properties should not return arrays",
            Justification = "Part of a Data Transfer Object")]
        public byte[] Checksum { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        [MaxLength(255)]
        public string Path { get; set; }
    }
}