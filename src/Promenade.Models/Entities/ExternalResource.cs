﻿using System.ComponentModel.DataAnnotations;
using Ocuda.Utility.Models;

namespace Ocuda.Promenade.Models.Entities
{
    public class ExternalResource
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        [MaxLength(255)]
        public string Url { get; set; }

        public int SortOrder { get; set; }
        public ExternalResourceType Type { get; set; }
    }
}
