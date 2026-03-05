using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class Subject
    {
        public Subject()
        {
            SubjectLanguages = [];
            SubjectEmedias = [];
        }

        [Key]
        [Required]
        public int Id { get; set; }

        [MaxLength(255)]
        [Required]
        public string Name { get; set; }

        [MaxLength(255)]
        [Required]
        public string Slug { get; set; }

        [NotMapped]
        public ICollection<string> SubjectEmedias { get; }

        [NotMapped]
        public ICollection<string> SubjectLanguages { get; }
    }
}