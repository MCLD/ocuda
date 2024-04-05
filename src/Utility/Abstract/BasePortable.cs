using System;

namespace Ocuda.Utility.Abstract
{
    public abstract class BasePortable
    {
        public int? ContentVersion { get; set; }
        public DateTime ExportedAt { get; set; }
        public string ExportedBy { get; set; }
        public string Source { get; set; }
        public string Type { get; set; }
        public int Version { get; set; }
    }
}