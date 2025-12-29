using System;
using Microsoft.AspNetCore.Http;

namespace Ocuda.Ops.Models
{
    [Serializable]
    public class SlideUploadJob
    {
        public string ApiKey { get; set; }
        public DateTime EndDate { get; set; }
        public IFormFile File { get; set; }
        public string Filepath { get; set; }
        public string Set { get; set; }
        public DateTime StartDate { get; set; }
        public int TimeZoneOffsetMinutes { get; set; }
    }
}