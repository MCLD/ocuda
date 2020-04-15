﻿using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Users
{
    public class MetadataViewModel
    {
        public ICollection<UserMetadataType> MetadataTypes { get; set; }
        public PaginateModel PaginateModel { get; set; }
        public UserMetadataType MetadataType { get; set; }
    }
}
