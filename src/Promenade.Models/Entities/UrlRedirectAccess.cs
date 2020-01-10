using System;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class UrlRedirectAccess
    {
        [Key]
        [Required]
        public int Id { get; set; }

        public int UrlRedirectId { get; set; }
        public UrlRedirect UrlRedirect { get; set; }
        public DateTime AccessDate { get; set; }
    }
}
