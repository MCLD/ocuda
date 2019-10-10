using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Clc.Polaris.Api.Models
{
    /// <summary>
    /// Protected token used to authenticate protected methods and public method overrides
    /// </summary>
    [XmlRoot(ElementName = "AuthenticationResult")]
    public class ProtectedToken : PapiResponseCommon
    {
        /// <summary>
        /// Access token
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Access secret
        /// </summary>
        public string AccessSecret { get; set; }

        /// <summary>
        /// Token expiration date
        /// </summary>
        [XmlElement(ElementName = "AuthExpDate")]
        public DateTime? ExpirationDate { get; set; }
    }
}
