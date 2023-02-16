using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.UserSync
{
    public class IndexViewModel : PaginateModel
    {
        public ICollection<UserSyncHistory> UserSyncHistories { get; set; }
    }
}