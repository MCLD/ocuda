using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clc.Polaris.Api.Models
{
    public class PatronAuthenticationResult : PapiResponseCommon
    {
        public string AccessToken { get; set; }
        public int PatronID { get; set; }
    }
}
