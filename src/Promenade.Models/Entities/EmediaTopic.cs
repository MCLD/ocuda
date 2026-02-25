using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class EmediaTopic
    {
        public Emedia Emedia { get; set; }

        [Key]
        [Required]
        public int EmediaId { get; set; }

        public Topic Topic { get; set; }

        [Key]
        [Required]
        public int TopicId { get; set; }
    }
}