using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Controllers.ViewModels.Profile
{
    public class UpdatePictureViewModel
    {
        public int CropHeight { get; set; }
        public int CropWidth { get; set; }
        public int DisplayDimension { get; set; }
        public string ProfilePicture { get; set; }
        public User User { get; set; }
        public int UserId { get; set; }
    }
}