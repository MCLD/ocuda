using System;

namespace Ocuda.Ops.Models
{
    [Serializable]
    public class UserInformation
    {
        public bool Authenticated { get; set; }
        public DateTime? AuthenticatedAt { get; set; }
        public string Username { get; set; }
    }
}