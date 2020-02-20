using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class Emedia
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [MaxLength(255)]
        [Required]
        public string Name { get; set; }

        [MaxLength(255)]
        [Required]
        public string Stub { get; set; }

        public int GroupId { get; set; }
        public EmediaGroup Group { get; set; }

        [MaxLength(255)]
        [Required]
        public string RedirectUrl { get; set; }

        [NotMapped]
        public ICollection<Category> Categories { get; set; }

        [NotMapped]
        public EmediaText EmediaText { get; set; }
    }
}
