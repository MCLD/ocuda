using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Ops.Models.Entities
{
    public class User
    {
        public int? AssociatedLocation { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }

        [MaxLength(255)]
        public string Email { get; set; }

        public int? EmployeeId { get; set; }
        public bool ExcludeFromRoster { get; set; }

        [NotMapped]
        public bool HasUpdates
        {
            get
            {
                return UpdatedAssociatedLocation
                    || UpdatedEmail
                    || UpdatedName
                    || UpdatedSupervisor
                    || UpdatedTitle
                    || UpdatedVacateDate;
            }
        }

        public int Id { get; set; }

        public bool IsDeleted { get; set; }

        public bool? IsInLatestRoster { get; set; }

        public bool IsSysadmin { get; set; }

        public DateTime? LastLdapCheck { get; set; }

        public DateTime? LastLdapUpdate { get; set; }

        public DateTime? LastRosterUpdate { get; set; }

        public DateTime? LastSeen { get; set; }

        public ICollection<UserMetadata> Metadata { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Nickname { get; set; }

        [NotMapped]
        public string Notes { get; set; }

        [MaxLength(255)]
        public string Phone { get; set; }

        [MaxLength(255)]
        public string PictureFilename { get; set; }

        public int? PictureUpdatedBy { get; set; }

        [NotMapped]
        public int? PriorAssociatedLocation { get; set; }

        [NotMapped]
        public string PriorEmail { get; set; }

        [NotMapped]
        public string PriorName { get; set; }

        [NotMapped]
        public string PriorSupervisorName { get; set; }

        [NotMapped]
        public string PriorTitle { get; set; }

        [NotMapped]
        public DateTime? PriorVacateDate { get; set; }

        public bool ReauthenticateUser { get; set; }

        public DateTime? ServiceStartDate { get; set; }

        public User Supervisor { get; set; }

        public int? SupervisorId { get; set; }

        [NotMapped]
        public string SupervisorName { get; set; }

        [MaxLength(255)]
        public string Title { get; set; }

        public bool UpdatedAssociatedLocation
        { get { return AssociatedLocation != PriorAssociatedLocation; } }

        public DateTime? UpdatedAt { get; set; }

        public int? UpdatedBy { get; set; }

        [NotMapped]
        public bool UpdatedEmail
        { get { return !string.Equals(Email, PriorEmail, StringComparison.OrdinalIgnoreCase); } }

        [NotMapped]
        public bool UpdatedName
        {
            get
            {
                return !string.Equals(Name, PriorName, StringComparison.OrdinalIgnoreCase);
            }
        }

        [NotMapped]
        public bool UpdatedSupervisor
        {
            get
            {
                return PriorSupervisorName != SupervisorName;
            }
        }

        [NotMapped]
        public bool UpdatedTitle
        { get { return !string.Equals(Title, PriorTitle, StringComparison.OrdinalIgnoreCase); } }

        [NotMapped]
        public bool UpdatedVacateDate
        {
            get
            {
                return VacateDate != PriorVacateDate;
            }
        }

        [MaxLength(255)]
        public string Username { get; set; }

        public DateTime? VacateDate { get; set; }
    }
}