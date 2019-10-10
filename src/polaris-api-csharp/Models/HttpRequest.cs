using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Clc.Polaris.Api
{
    /// <summary>
    /// Class to hold HttpRequestMessage data
    /// </summary>
    public class HttpRequest
    {
        /// <summary>
        /// The request body
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Request headers
        /// </summary>
        public HttpRequestHeaders Headers { get; }

        /// <summary>
        /// Request method
        /// </summary>
        public HttpMethod Method { get; set; }

        /// <summary>
        /// Properties
        /// </summary>
        public IDictionary<string, object> Properties { get; }

        /// <summary>
        /// Request URL
        /// </summary>
        public Uri RequestUri { get; set; }

        /// <summary>
        /// Version
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public HttpRequest()
        {

        }

        /// <summary>
        /// Copies data from request parameter
        /// </summary>
        /// <param name="request"></param>
        public HttpRequest(HttpRequestMessage request)
        {
            Content = request.Content == null ? null : request.Content.ReadAsStringAsync().Result;
            Headers = request.Headers;
            Method = request.Method;
            Properties = request.Properties;
            RequestUri = request.RequestUri;
            Version = request.Version;
        }
    }
}
