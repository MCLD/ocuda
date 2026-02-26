using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class Emedia
    {
        public Emedia()
        {
            Categories = [];
            EmediaLanguages = [];
            Topics = [];
        }

        [NotMapped]
        public ICollection<Category> Categories { get; }

        [NotMapped]
        public ICollection<string> EmediaLanguages { get; }

        [NotMapped]
        public EmediaText EmediaText { get; set; }

        public EmediaGroup Group { get; set; }

        public int GroupId { get; set; }

        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public bool IsAvailableExternally { get; set; }

        [Required]
        public bool IsHttpPost { get; set; }

        [MaxLength(255)]
        [Required]
        public string Name { get; set; }

        [DisplayName("Redirect Url")]
        [MaxLength(255)]
        [Required]
        public string RedirectUrl { get; set; }

        [MaxLength(255)]
        [Required]
        public string Slug { get; set; }

        [NotMapped]
        public ICollection<Topic> Topics { get; }
    }
}