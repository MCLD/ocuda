using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Posts
{
    public class DetailViewModel
    {
        public Post Post { get; set; }
        public int SectionId { get; set; }
        public IEnumerable<File> Attachments { get; set; }
    }
}
