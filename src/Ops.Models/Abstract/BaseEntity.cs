using System;
using System.ComponentModel.DataAnnotations.Schema;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Models.Abstract
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules",
        "SA1402:File may only contain a single type",
        Justification = "Type with generic and non-generic versions.")]
    public class BaseEntity<T>
    {
        public DateTime CreatedAt { get; set; }

        [ForeignKey(nameof(CreatedByUser))]
        public int CreatedBy { get; set; }

        [NotMapped]
        public string CreatedByName { get; set; }

        public User CreatedByUser { get; set; }

        public T Id { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(UpdatedByUser))]
        public int? UpdatedBy { get; set; }

        [NotMapped]
        public string UpdatedByName { get; set; }

        public User UpdatedByUser { get; set; }
    }

    public class BaseEntity : BaseEntity<int>
    {
    }
}
