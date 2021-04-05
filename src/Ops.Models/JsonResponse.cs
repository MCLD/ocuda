using System;

namespace Ocuda.Ops.Models
{
    [Serializable]
    public class JsonResponse
    {
        public string Message { get; set; }
        public bool ServerResponse { get; set; }
        public bool Success { get; set; }
        public string Url { get; set; }
    }
}