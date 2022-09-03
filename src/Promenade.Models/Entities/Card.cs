using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class Card
    {
        public Deck Deck { get; set; }

        [Required]
        public int DeckId { get; set; }

        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public int Order { get; set; }
    }
}