using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Ocuda.Ops.Models.Abstract;

namespace Ocuda.Ops.Models.Entities
{
    public class BooksByMailComment : BaseEntity
    {
        public int CustomerId { get; set; }
        public BooksByMailCustomer Customer { get; set; }

        [Required]
        [DisplayName("Comment")]
        public string Text { get; set; }
    }
}