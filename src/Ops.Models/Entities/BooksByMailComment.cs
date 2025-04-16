using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Ops.Models.Entities
{
    [Table("BooksByMailComments")]
    public class BooksByMailComment
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public BooksByMailCustomer Customer { get; set; }
        public DateTime CreatedAt { get; set; }

        [Required]
        [MaxLength(255)]
        public string StaffUsername { get; set; }

        [Required]
        [DisplayName("Comment")]
        public string Text { get; set; }
    }
}