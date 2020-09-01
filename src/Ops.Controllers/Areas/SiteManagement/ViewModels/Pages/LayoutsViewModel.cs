﻿using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Pages
{
    public class LayoutsViewModel
    {
        public ICollection<PageLayout> PageLayouts { get; set; }
        public PaginateModel PaginateModel { get; set; }
        public PageLayout PageLayout { get; set; }

        public int HeaderId { get; set; }

        [DisplayName("Page Name")]
        public string HeaderName { get; set; }

        [DisplayName("Stub")]
        public string HeaderStub { get; set; }

        [DisplayName("Type")]
        public PageType HeaderType { get; set; }

        public SelectList SocialCardList { get; set; }

        public bool IsSiteManager { get; set; }
    }
}
