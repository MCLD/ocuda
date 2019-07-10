using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ocuda.Promenade.Models.Entities
{
    class LocationsFeatures
    {
        [Required]
        public int LocationId { get; set; }

        [Required]
        public int FeatureId { get; set; }

        [MaxLength(255)]
        public string Text { get; set; }

        [MaxLength(255)]
        public string RedirectUrl { get; set; }
    }
}
