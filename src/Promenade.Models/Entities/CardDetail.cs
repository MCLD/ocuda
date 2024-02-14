using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class CardDetail
    {
        [DisplayName("Image alternative text")]
        [Description("How should this image be described to someone who can't see it?")]
        [MaxLength(255)]
        public string AltText { get; set; }

        public Card Card { get; set; }

        [Key]
        [Required]
        public int CardId { get; set; }

        [MaxLength(255)]
        public string Filename { get; set; }

        [MaxLength(255)]
        [DisplayName("Footer (optional)")]
        public string Footer { get; set; }

        [MaxLength(255)]
        [DisplayName("Footer Link (optional)")]
        public string FooterLink { get; set; }

        [MaxLength(255)]
        [DisplayName("Header (optional)")]
        public string Header { get; set; }

        [NotMapped]
        public string ImagePath { get; set; }

        public Language Language { get; set; }

        [NotMapped]
        public int LanguageCount { get; set; }

        [Key]
        [Required]
        [DisplayName("Language")]
        public int LanguageId { get; set; }

        [MaxLength(255)]
        [DisplayName("Link (optional)")]
        public string Link { get; set; }

        [DisplayName("Text (optional)")]
        public string Text { get; set; }
    }
}