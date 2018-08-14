using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Ops.Models.Abstract
{
    public class BaseEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }

        [ForeignKey(nameof(CreatedByUser))]
        public int CreatedBy { get; set; }
        public User CreatedByUser { get; set; }
    }
}
