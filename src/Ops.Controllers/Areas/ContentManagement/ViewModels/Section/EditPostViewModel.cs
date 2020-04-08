using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Section
{
    public class EditPostViewModel
    {
        public Post Post { get; set; }

        public string SectionStub { get; set; }

        public int SectionId { get; set; }

        public string SectionName { get; set; }

        public SelectList SelectionPostCategories { get; set; }

        [DisplayName("Post Categories")]
        public List<int> CategoryIds { get; set; }

        public List<PostCategory> PostCategories { get; set; }
    }
}
