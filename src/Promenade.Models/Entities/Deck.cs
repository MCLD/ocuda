using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class Deck
    {
        public ICollection<Card> Cards { get; set; }

        [Key]
        [Required]
        public int Id { get; set; }

        [MaxLength(255)]
        [Required]
        public string Name { get; set; }
    }
}