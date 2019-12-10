using System.ComponentModel.DataAnnotations;
using Ocuda.Promenade.Models.Abstract;

namespace Ocuda.Promenade.Models.Entities
{
    public class UrlRedirect : BaseEntity
    {
        public bool IsActive { get; set; }
        public bool IsPermanent { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        [MaxLength(255)]
        public string RequestPath { get; set; }

        [MaxLength(500)]
        public string Url { get; set; }
    }
}
