using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Ops.Models.Entities
{
    public class DigitalDisplay : Abstract.BaseEntity
    {
        [MaxLength(255)]
        [Display(Name = "Username and password in the format username:password (optional)")]
        public string BasicAuthentication { get; set; }

        public DateTime? LastAttempt { get; set; }
        public DateTime? LastCommunication { get; set; }
        public DateTime? LastContentVerification { get; set; }

        [MaxLength(255)]
        [Display(Name = "Description of the digital display locaton (optional)")]
        public string LocationDescription { get; set; }

        [Display(Name = "Location of this digital display (optional)")]
        public int? LocationId { get; set; }

        [Required]
        [MaxLength(255)]
        [Display(Name = "Name of this digital display")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "URI to access the display")]
        public Uri RemoteAddress { get; set; }

        public int SlideCount { get; set; }

        [NotMapped]
        public (DateTime AsOf, string Message) Status { get; set; }
    }
}