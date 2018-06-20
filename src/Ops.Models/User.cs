using System;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models
{
    public class User
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }

        [Required]
        public string Username { get; set; }
        public bool IsSysadmin { get; set; }
        public DateTime? LastRosterUpdate { get; set; }
    }
}
