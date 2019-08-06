using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class UrlRedirect
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public bool IsPermanent { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        [MaxLength(255)]
        public string RequestPath { get; set; }

        [MaxLength(255)]
        public string QueryKey { get; set; }

        [MaxLength(255)]
        public string QueryValue { get; set; }

        [MaxLength(500)]
        public string Url { get; set; }
    }
}
