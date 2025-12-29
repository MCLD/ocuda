using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Ops.Models.Entities
{
    public class CardRenewalResponse : Abstract.BaseEntity
    {
        [DisplayName("Email")]
        public int? EmailSetupId { get; set; }
        public EmailSetup EmailSetup { get; set; }

        public bool IsDeleted { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [NotMapped]
        public string Text { get; set; }

        public ResponseType Type { get; set; }
        public int SortOrder { get; set; }

        public enum ResponseType
        {
            Accept,
            Deny
        }
    }
}
