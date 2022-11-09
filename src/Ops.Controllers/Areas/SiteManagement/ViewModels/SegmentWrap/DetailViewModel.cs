namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.SegmentWrap
{
    public class DetailViewModel
    {
        public bool IsEdit { get; set; }
        public int Page { get; set; }
        public Promenade.Models.Entities.SegmentWrap SegmentWrap { get; set; }
        public int SegmentWrapId { get; set; }
    }
}