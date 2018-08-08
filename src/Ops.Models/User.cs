using System;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models
{
    public class User
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }

        public string Username { get; set; }
        public bool IsSysadmin { get; set; }
        public DateTime? LastLdapCheck { get; set; }
        public DateTime? LastLdapUpdate { get; set; }
        public DateTime? LastRosterUpdate { get; set; }

        public bool ReauthenticateUser { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }
        [MaxLength(255)]
        public string Nickname { get; set; }
        [MaxLength(255)]
        public string Title { get; set; }
        [MaxLength(255)]
        public string Email { get; set; }
        public string Phone { get; set; }
        public int? EmployeeId { get; set; }
        public DateTime? ServiceStartDate { get; set; }

        public int? SupervisorId { get; set; }
        public User Supervisor { get; set; }
        public DateTime? LastSeen { get; set; }

        public bool? IsInLatestRoster { get; set; }
        public bool IsDeleted { get; set; }
    }
}
