using Ocuda.Promenade.Models.Abstract;

namespace Ocuda.Promenade.Models.Entities
{
    public class LocationGroup : BaseEntity
    {
        public int LocationId { get; set; }

        public int GroupId { get; set; }

        public bool HasSubscription { get; set; }
    }
}
