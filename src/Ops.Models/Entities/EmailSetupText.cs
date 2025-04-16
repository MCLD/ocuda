using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    public class EmailSetupText
    {
        [Key]
        [Required]
        public int EmailSetupId { get; set; }

        public EmailSetup EmailSetup { get; set; }

        [Key]
        [Required]
        [MaxLength(255)]
        public string PromenadeLanguageName { get; set; }

        [MaxLength(255)]
        public string UrlParameters { get; set; }

        [MaxLength(255)]
        public string Preview { get; set; }

        public string BodyText { get; set; }
        public string BodyHtml { get; set; }

        [MaxLength(255)]
        public string Subject { get; set; }
    }
}