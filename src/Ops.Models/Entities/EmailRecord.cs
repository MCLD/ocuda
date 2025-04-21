using System;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    public class EmailRecord : Utility.Email.Record
    {
        public EmailRecord() { }
        public EmailRecord(Utility.Email.Record incoming) : base(incoming) { }

        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
