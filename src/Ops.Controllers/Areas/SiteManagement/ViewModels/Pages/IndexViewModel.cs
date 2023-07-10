using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Pages
{
    public class IndexViewModel
    {
        public string BaseLink { get; set; }
        public SelectList CarouselTemplates { get; set; }
        public SelectList ImageFeatureTemplates { get; set; }
        public bool IsSiteManager { get; set; }
        public bool IsWebContentManager { get; set; }
        public PageHeader PageHeader { get; set; }
        public ICollection<PageHeader> PageHeaders { get; set; }
        public PageType PageType { get; set; }
        public PaginateModel PaginateModel { get; set; }
        public IList<string> PermissionIds { get; set; }

        public bool ShowAddPageButton
        {
            get
            {
                return IsSiteManager
                    && PageType != PageType.Home;
            }
        }

        public string TypeNotes { get; set; }

        public string MakeLink(PageHeader pageHeader)
        {
            if (pageHeader == null)
            {
                return null;
            }

            string type = pageHeader.Type.ToString().Trim('/').ToLowerInvariant();
            string stub = pageHeader.Stub.Trim('/');

            if (!string.IsNullOrEmpty(BaseLink))
            {
                if (type == "home" && stub == "index")
                {
                    return BaseLink;
                }
                else
                {
                    return BaseLink.TrimEnd('/') + '/' + type + '/' + stub;
                }
            }
            else
            {
                return null;
            }
        }
    }
}