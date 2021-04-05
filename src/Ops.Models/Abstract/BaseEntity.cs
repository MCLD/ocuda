using System;
using System.ComponentModel.DataAnnotations.Schema;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Models.Abstract
{
    public class BaseEntity
    {
        public DateTime CreatedAt { get; set; }

        [ForeignKey(nameof(CreatedByUser))]
        public int CreatedBy { get; set; }

        [NotMapped]
        public string CreatedByName { get; set; }

        public User CreatedByUser { get; set; }
        public int Id { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(UpdatedByUser))]
        public int? UpdatedBy { get; set; }

        public User UpdatedByUser { get; set; }
    }
}