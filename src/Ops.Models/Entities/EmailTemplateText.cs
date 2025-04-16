using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    public class EmailTemplateText
    {
        [Key]
        [Required]
        public int EmailTemplateId { get; set; }

        public EmailTemplate EmailTemplate { get; set; }

        [Key]
        [Required]
        [MaxLength(255)]
        public string PromenadeLanguageName { get; set; }

        [Required]
        public string TemplateHtml { get; set; }

        [Required]
        public string TemplateMjml { get; set; }

        [Required]
        public string TemplateText { get; set; }
    }
}