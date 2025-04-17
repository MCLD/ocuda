using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Ops.Models.Entities
{
    [Table("BooksByMailCustomers")]
    public class BooksByMailCustomer
    {
        public int Id { get; set; }
        public int CustomerLookupID { get; set; }
        public string Likes { get; set; }
        public string Dislikes { get; set; }
        public string Notes { get; set; }

        public ICollection<BooksByMailComment> Comments { get; set; }
    }
}