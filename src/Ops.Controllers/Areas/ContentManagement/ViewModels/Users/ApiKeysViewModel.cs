using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Users
{
    public class ApiKeysViewModel : PaginateModel
    {
        public ApiKeysViewModel()
        {
            ApiKeys = [];
        }

        public IList<ApiKey> ApiKeys { get; }
    }
}