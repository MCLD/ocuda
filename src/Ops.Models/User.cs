using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ocuda.Ops.Models
{
    public class User : Abstract.BaseEntity
    {
        [Required]
        public string Username { get; set; }
        public bool IsSysadmin { get; set; }
        public DateTime? LastRosterUpdate { get; set; }
    }
}
