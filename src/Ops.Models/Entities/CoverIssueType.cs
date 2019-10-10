using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Ops.Models.Abstract;

namespace Ocuda.Ops.Models.Entities
{
    public class CoverIssueType : BaseEntity
    {
        public string Name { get; set; }
        public bool HasMessage { get; set; }
    }
}
