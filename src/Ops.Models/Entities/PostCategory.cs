using System;
using System.Collections.Generic;
using System.Text;

namespace Ocuda.Ops.Models.Entities
{
    public class PostCategory : Abstract.BaseEntity
    {
        public int SectionId { get; set; }

        public string Name { get; set; }

        public string Stub { get; set; }
    }
}
