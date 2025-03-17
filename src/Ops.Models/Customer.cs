using System;
using System.Collections.Generic;

namespace BooksByMail.Data.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public int PatronID { get; set; }

        public string Likes { get; set; }
        public string Dislikes { get; set; }
        public string Notes { get; set; }

        public ICollection<Comment> Comments { get; set; }
    }
}
