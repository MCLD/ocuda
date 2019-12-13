using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Ocuda.Promenade.Models.Abstract;

namespace Ocuda.Promenade.Models.Entities
{
    public class EmediaCategory : BaseEntity
    {
        [Required]
        public int EmediaId { get; set; }

        [Required]
        public int CategoryId { get; set; }
    }
}
