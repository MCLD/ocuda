using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Ops.Models.Abstract
{
    public class BaseEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }

        [ForeignKey(nameof(User))]
        public int CreatedBy { get; set; }
        public User User { get; set; }
    }
}
