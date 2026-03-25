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
            Subjects = [];
        }

        [NotMapped]
        public ICollection<Category> Categories { get; }

        [NotMapped]
        public ICollection<string> EmediaLanguages { get; }

        [NotMapped]
        public EmediaText EmediaText { get; set; }

        public EmediaGroup Group { get; set; }

        public int? GroupId { get; set; }

        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        [DefaultValue(true)]
        public bool IsActive { get; set; }

        [Required]
        [DisplayName("Available outside our network")]
        public bool IsAvailableExternally { get; set; }

        [Required]
        [DisplayName("HTTP Method")]
        public bool IsHttpPost { get; set; }

        [MaxLength(255)]
        [Required]
        public string Name { get; set; }

        [DisplayName("Redirect Url")]
        [MaxLength(255)]
        [Required]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
            "CA1056:URI-like properties should not be strings",
            Justification = "URL stored as string in database server")]
        public string RedirectUrl { get; set; }

        [MaxLength(255)]
        [Required]
        public string Slug { get; set; }

        [NotMapped]
        public ICollection<Subject> Subjects { get; }
    }
}