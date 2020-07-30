using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class Podcast
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [MaxLength(255)]
        public string Title { get; set; }

        [MaxLength(255)]
        public string Subtitle { get; set; }

        [MaxLength(2000)]
        public string Description { get; set; }

        [MaxLength(255)]
        public string Author { get; set; }

        [MaxLength(255)]
        public string Link { get; set; }

        [MaxLength(32)]
        public string Language { get; set; }

        [MaxLength(255)]
        public string ITunesName { get; set; }

        [MaxLength(255)]
        public string ITunesEmail { get; set; }

        public bool Explicit { get; set; }

        [MaxLength(255)]
        public string Image { get; set; }

        [MaxLength(255)]
        public string Category { get; set; }
    }
}
