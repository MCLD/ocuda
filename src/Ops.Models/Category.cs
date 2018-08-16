using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Ocuda.Ops.Models
{
    public class Category : Abstract.BaseEntity
    {
        public string Name { get; set; }
        public int SectionId { get; set; }
        public Section Section { get; set; }

        [Column(TypeName = "int")]
        public CategoryType CategoryType { get; set; }

        public bool IsAttachment { get; set; }
        public bool IsDefault { get; set; }
        public bool IsNavigation { get; set; }

        [DisplayName("Thumbnails")]
        public bool ThumbnailRequired { get; set; }

        public ICollection<CategoryFileType> CategoryFileTypes { get; set; }
    }

    public enum CategoryType
    {
        File,
        Link
    }

}
