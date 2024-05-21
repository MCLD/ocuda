using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.ViewModels.Locations
{
    public class LinkViewModel
    {
        public Feature Feature { get; set; }
        public int FeatureId { get; set; }
        public string Link { get; set; }
        public Location Location { get; set; }
        public string LocationStub { get; set; }
        public bool NewTab { get; set; }
    }
}