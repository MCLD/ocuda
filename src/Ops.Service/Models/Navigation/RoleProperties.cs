namespace Ocuda.Ops.Service.Models.Navigation
{
    public class RoleProperties
    {
        public bool CanHaveChildren { get; set; }
        public bool CanHaveGrandchildren { get; set; }
        public bool CanHaveText { get; set; }

        public bool ChildrenCanChangeToLink { get; set; }
        public bool ChildrenCanDisplayIcon { get; set; }
        public bool ChildrenCanHideText { get; set; }
        public bool ChildrenCanTargetNewWindow { get; set; }
    }
}
