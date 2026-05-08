using System;

namespace Ocuda.Ops.Models
{
    [Serializable]
    public class JsonResponse
    {
        public string Message { get; set; }
        public bool ServerResponse { get; set; }
        public bool Success { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
            "CA1056:URI-like properties should not be strings",
            Justification = "This value is passed through JSON as a string")]
        public string Url { get; set; }
    }

    [Serializable]
    public class JsonResponse<T>
    {
        public T Data { get; set; }
        public string Message { get; set; }
        public bool ServerResponse { get; set; }
        public string Source { get; set; }
        public bool Success { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
            "CA1056:URI-like properties should not be strings",
            Justification = "This value is passed through JSON as a string")]
        public string Url { get; set; }
    }
}