using System.Collections.Generic;
using Ocuda.Ops.Models.Abstract;

namespace Ocuda.Ops.Models.Entities
{
    public class BooksByMailCustomer : BaseEntity
    {
        public IEnumerable<BooksByMailComment> Comments { get; set; }
        public string Dislikes { get; set; }
        public int ExternalCustomerId { get; set; }
        public string Likes { get; set; }
        public string Notes { get; set; }
    }
}