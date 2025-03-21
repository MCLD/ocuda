using System;

namespace BooksByMail.Models
{
    public class Material
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string Category { get; set; }
        public string HoldStatus { get; set; }
        public DateTime CheckoutDate { get; set; }
        public DateTime DueDate { get; set; }
    }
}
