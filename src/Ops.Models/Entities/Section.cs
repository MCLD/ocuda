using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ocuda.Ops.Models.Entities
{
    public class Section : Abstract.BaseEntity
    {
        public string Name { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string EmbedVideoUrl { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string Icon { get; set; }

        [Required]
        public string Stub { get; set; }
    }
}
