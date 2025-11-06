using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ocuda.Ops.Models.Abstract;

namespace Ocuda.Ops.Models.Entities
{
    public class ApiKey : BaseEntity
    {
        public DateTime? EndDate { get; set; }

        [Required]
        [MaxLength(8)]
        public string Identifier { get; set; }

        [Required]
        [MaxLength(32)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance",
            "CA1819:Properties should not return arrays",
            Justification = "The key's is stored in the database as bytes")]
        public byte[] Key { get; set; }

        [Required]
        public ApiKeyType KeyType { get; set; }

        public DateTime? LastUsed { get; set; }

        public User RepresentsUser { get; set; }

        [Required]
        public int RepresentsUserId { get; set; }

        [NotMapped]
        public string RepresentsUserName { get; set; }
    }
}