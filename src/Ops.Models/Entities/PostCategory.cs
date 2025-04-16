namespace Ocuda.Ops.Models.Entities
{
    public class PostCategory
    {
        public int PostId { get; set; }
        public Post Post { get; set; }

        public int CategoryId { get; set; }

        public Category Category { get; set; }
    }
}