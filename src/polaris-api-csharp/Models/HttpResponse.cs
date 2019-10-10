using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Clc.Polaris.Api
{
    /// <summary>
    /// Class to hold HttpResponseMessage data
    /// </summary>
    public class HttpResponse
    {
        /// <summary>
        /// Response content
        /// </summary>
        public string Content { get; set; }
        private HttpContent _content { get; set; }

        /// <summary>
        /// Headers
        /// </summary>
        public HttpResponseHeaders Headers { get; }

        /// <summary>
        /// Does status code indicate success
        /// </summary>
        public bool IsSuccessStatusCode { get; }

        /// <summary>
        /// Reason phrase
        /// </summary>
        public string ReasonPhrase { get; set; }

        /// <summary>
        /// Request that generated the response
        /// </summary>
        public HttpRequestMessage RequestMessage { get; set; }

        /// <summary>
        /// Status code
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Version
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public HttpResponse()
        {

        }

        /// <summary>
        /// Copies data from response parameter
        /// </summary>
        /// <param name="response"></param>
        public HttpResponse(HttpResponseMessage response)
        {
            Content = response.Content.ReadAsStringAsync().Result;
            _content = response.Content;
            Headers = response.Headers;
            IsSuccessStatusCode = response.IsSuccessStatusCode;
            ReasonPhrase = response.ReasonPhrase;
            RequestMessage = response.RequestMessage;
            StatusCode = response.StatusCode;
            Version = response.Version;
        }

        /// <summary>
        /// Provides some basic response data
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"StatusCode: {StatusCode}, Content-Type: {_content.Headers.ContentType}, Content-Length: {Content.Length}";
        }
    }
}
