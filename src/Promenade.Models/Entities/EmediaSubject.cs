using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class EmediaSubject
    {
        public Emedia Emedia { get; set; }

        [Key]
        [Required]
        public int EmediaId { get; set; }

        public Subject Subject { get; set; }

        [Key]
        [Required]
        public int SubjectId { get; set; }
    }
}