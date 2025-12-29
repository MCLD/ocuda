using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Users
{
    public class AddApiKeyViewModel
    {
        public AddApiKeyViewModel()
        {
            ApiKeyTypesSelectList = [];
        }

        public int ActAsUserId { get; set; }

        [DisplayName("API Key type")]
        public ApiKeyType ApiKeyType { get; set; }

        public IList<SelectListItem> ApiKeyTypesSelectList { get; }

        [DisplayName("End date (optional)")]
        public DateTime? EndDate { get; set; }

        public Uri JsonStaffSearchUri { get; set; }
    }
}