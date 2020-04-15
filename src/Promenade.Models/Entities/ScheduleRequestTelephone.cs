using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class ScheduleRequestTelephone
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Phone { get; set; }
    }
}
