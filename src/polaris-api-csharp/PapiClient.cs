using Clc.Polaris.Api.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Clc.Polaris.Api
{
    /// <summary>
    /// Polaris API helper class
    /// </summary>
    public partial class PapiClient
    {
        /// <summary>
        /// Your PAPI Access ID
        /// </summary>
        public string AccessID { get; set; }

        /// <summary>
        /// Your PAPI Access Key
        /// </summary>
        public string AccessKey { get; set; }

        /// <summary>
        /// The base URL of your PAPI service
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// The staff credentials used for protected methods and public method overrides
        /// </summary>
        public PolarisUser StaffOverrideAccount { get; set; }

        private static ProtectedToken _token;

        /// <summary>
        /// Used for protected methods and public method overrides
        /// </summary>
        public ProtectedToken Token
        {
            get
            {
                if (_token == null || _token.ExpirationDate <= DateTime.Now)
                {
                    _token = AuthenticateStaffUser(StaffOverrideAccount.Domain, StaffOverrideAccount.Username, StaffOverrideAccount.Password).Data;
                }
                return _token;
            }
            set { _token = value; }
        }

        HttpClient client = new HttpClient();

        /// <summary>
        /// Default constructor
        /// </summary>
        public PapiClient()
        {
        }

        /// <summary>
        /// Execute a PAPI request
        /// </summary>
        /// <typeparam name="T">Response data type</typeparam>
        /// <param name="method">HTTP method of the PAPI method</param>
        /// <param name="url">The URL of the method, beginning with /PAPIService</param>
        /// <param name="pin">The patron's pin for public or protected token Access Secret for protected methods</param>
        /// <param name="body">The XML request body, if any</param>
        /// <param name="isOverride">Execute as an override request</param>
        /// <returns>A PapiResponse object with a deserialized data object of type T</returns>
        public PapiResponse<T> Execute<T>(HttpMethod method, string url, string pin = null, string body = null, bool isOverride = false)
        {
            var request = CreateRequest(method, url, pin, body, isOverride);
            var papiResponse = new PapiResponse<T>(request);
            var response = client.SendAsync(request).Result;
            papiResponse.Response = new HttpResponse(response);
            if (response.IsSuccessStatusCode)
            {
                papiResponse.Data = (T)new XmlSerializer(typeof(T)).Deserialize(response.Content.ReadAsStreamAsync().Result);
            }

            return papiResponse;
        }

        /// <summary>
        /// Perform an override PAPI request
        /// </summary>
        /// <typeparam name="T">Response data type</typeparam>
        /// <param name="method">HTTP method of the PAPI method</param>
        /// <param name="url">The URL of the method, beginning with /PAPIService</param>
        /// <param name="body">The XML request body, if any</param>
        /// <returns>A PapiResponse object with a deserialized data object of type T</returns>
        public PapiResponse<T> OverrideExecute<T>(HttpMethod method, string url, string body = null)
        {
            return Execute<T>(method, url, null, body, true);
        }

        /// <summary>
        /// Perform a PAPI request
        /// </summary>
        /// <param name="method">HTTP method of the PAPI method</param>
        /// <param name="url">The URL of the method, beginning with /PAPIService</param>
        /// <param name="pin">The patron's pin for public or protected token Access Secret for protected methods</param>
        /// <param name="body">The XML request body, if any</param>
        /// <param name="isOverride">Execute as an override request</param>
        /// <returns>A string of the response XML</returns>
        public string Execute(HttpMethod method, string url, string pin = null, string body = null, bool isOverride = false)
        {
            var request = CreateRequest(method, url, pin, body, isOverride);
            var response = client.SendAsync(request).Result;
            return response.Content.ReadAsStringAsync().Result;
        }

        /// <summary>
        /// Perform an override PAPI request
        /// </summary>
        /// <param name="method">HTTP method of the PAPI method</param>
        /// <param name="url">The URL of the method, beginning with /PAPIService</param>
        /// <returns>A string of the response XML</returns>
        public string OverrideExecute(HttpMethod method, string url)
        {
            return Execute(method, url, null, null, true);
        }

        HttpRequestMessage CreateRequest(HttpMethod method, string url, string pin = null, string body = null, bool isOverride = false)
        {
            var password = isOverride ? Token.AccessSecret : pin;
            url = BaseUrl.TrimEnd('/') + url.ToString();
            var request = new HttpRequestMessage(method, url);
            if (!string.IsNullOrWhiteSpace(body))
            {
                request.Content = new StringContent(body, Encoding.UTF8, "application/xml");
            }

            var gmtTime = DateTime.Now.ToUniversalTime().ToString("R");
            var hash = GetPAPIHash(AccessKey, request.Method.ToString(), url, gmtTime, password);
            request.Headers.Add("PolarisDate", gmtTime);
            request.Headers.Add("Authorization", string.Format("PWS {0}:{1}", AccessID, hash));
            if (isOverride)
            {
                request.Headers.Add("X-PAPI-AccessToken", Token.AccessToken);
            }
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            return request;
        }

        private static string GetPAPIHash(string accessKey, string httpMethod, string uri, string httpDate, string password)
        {
            byte[] secretBytes = Encoding.UTF8.GetBytes(accessKey);
            var hmac = new HMACSHA1(secretBytes);
            var hashString = httpMethod + uri + httpDate + password;
            byte[] dataBytes = Encoding.UTF8.GetBytes(hashString);
            byte[] computedHash = hmac.ComputeHash(dataBytes);
            return Convert.ToBase64String(computedHash);
        }

        static string PolarisEncode(string value)
        {
            var encoded = Uri.EscapeDataString(value);
            var encodedFixed = Regex.Replace(encoded, "(%[0-9a-f][0-9a-f])", c => c.Value.ToUpper());
            return encodedFixed.Replace("%20", "+").Replace("%3D", "=").Replace("%2F", "/").Replace("%40", "@").Replace("%2C", ",").Replace("%26", "&");
        }
    }
}