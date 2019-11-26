using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels
{
    public class AddPostViewModel
    {
        public Post Post { get; set; }

        public string SectionStub { get; set; }

        public int SectionId { get; set; }

        public string SectionName { get; set; }

        public SelectList SelectionPostCategories { get; set; }

        public List<Category> SectionCategories { get; set; }

        [DisplayName("Post Categories")]
        public List<int> CategoryIds { get; set; }
    }
}
