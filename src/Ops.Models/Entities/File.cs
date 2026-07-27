using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Ops.Models.Entities
{
    public class File : Abstract.BaseEntity
    {
        public File()
        {
            Thumbnails = [];
        }

        [MaxLength(255)]
        public string Description { get; set; }

        [Display(Name = "Associated date")]
        public DateTime? FileDate { get; set; }

        public FileLibrary FileLibrary { get; set; }

        public int FileLibraryId { get; set; }

        public FileType FileType { get; set; }

        public int FileTypeId { get; set; }

        [NotMapped]
        public string FullName
        {
           get
            {
                return $"{Name}{FileType?.Extension}";
            }
        }

        [NotMapped]
        public DateTime LastUpdateAt
        {
            get
            {
                return UpdatedAt ?? CreatedAt;
            }
        }

        [NotMapped]
        public User LastUpdateBy
        {
            get
            {
                return UpdatedByUser ?? CreatedByUser;
            }
        }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [NotMapped]
        public string Size { get; set; }

        public ICollection<FileThumbnail> Thumbnails { get; }
    }
}
