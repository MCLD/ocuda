using System;
using System.Collections.Generic;
using System.Text;

namespace Ocuda.Ops.Models.Entities
{
    public class UserMetadata
    {
        public int UserId { get; set; }
        public User User { get; set; }

        public int UserMetadataTypeId { get; set; }
        public UserMetadataType UserMetadataType { get; set; }
    }
}
