﻿using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Controllers.ViewModels.Post
{
    public class AdminDetailViewModel
    {
        public Models.Post Post { get; set; }
        public string Action { get; set; }
    }
}
