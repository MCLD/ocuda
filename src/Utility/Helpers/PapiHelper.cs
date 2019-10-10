using Clc.Polaris.Api;
using Clc.Polaris.Api.Models;
using Microsoft.Extensions.Configuration;

namespace Ocuda.Utility.Helpers
{
    public class PapiHelper : PapiClient
    {
        public int UserId { get; set; }
        public int WorkstationId { get; set; }
        public int PickupOrgId { get; set; }
        public int RequestingOrgId { get; set; }

        public PapiHelper(IConfiguration config)
        {
            AccessID = config["PAPI:AccessId"];
            AccessKey = config["PAPI:AccessKey"];
            BaseUrl = config["PAPI:BaseUrl"];
            StaffOverrideAccount = new PolarisUser
            {
                Domain = config["PAPI:Domain"],
                Username = config["PAPI:Username"],
                Password = config["PAPI:Password"]
            };

            UserId = config.GetValue<int>("PAPI:UserId");
            WorkstationId = config.GetValue<int>("PAPI:WorkstationId");
        }

        public bool IsConfigured()
        {
            return !string.IsNullOrEmpty(AccessID)
                && !string.IsNullOrEmpty(AccessKey)
                && !string.IsNullOrEmpty(BaseUrl)
                && StaffOverrideAccount != null
                && UserId > 0
                && WorkstationId > 0;
        }
    }
}
