using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Files
{
    public class DetailViewModel
    {
        public File File { get; set; }
        public IEnumerable<Category> Categories { get; set; }
        public string Action { get; set; }
        public int SectionId { get; set; }
        public IFormFile FileData { get; set; }
        public ICollection<IFormFile> ThumbnailFiles { get; set; }
        public int MaxThumbnailCount { get; set; }
        public int[] ThumbnailIds { get; set; }
    }
}
