using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    public class EmailSetup
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Description { get; set; }

        [Required]
        [MaxLength(255)]
        public string FromEmailAddress { get; set; }

        [Required]
        [MaxLength(255)]
        public string FromName { get; set; }

        [Required]
        public int EmailTemplateId { get; set; }
        public EmailTemplate EmailTemplate { get; set; }
    }
}
