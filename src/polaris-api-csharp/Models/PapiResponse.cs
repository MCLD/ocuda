using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Clc.Polaris.Api.Models
{
    /// <summary>
    /// A PAPI response
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PapiResponse<T>
    {
        /// <summary>
        /// Deserialized data object from response XML
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// The raw response from the PAPI service
        /// </summary>
        public HttpResponse Response { get; set; }

        /// <summary>
        /// The request that was sent to the PAPI service
        /// </summary>
        public HttpRequest Request { get; set; }

        public Exception Exception { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public PapiResponse()
        {

        }

        /// <summary>
        /// Copy's request data into a new object to allow reading of request body data
        /// </summary>
        /// <param name="request"></param>
        public PapiResponse(HttpRequestMessage request)
        {
            Request = new HttpRequest(request);
        }

        /// <summary>
        /// Returns the data object's ToString method
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Data.ToString();
        }
    }
}
