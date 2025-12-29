using System.Threading.Tasks;
using Clc.Polaris.Api.Models;
using Ocuda.PolarisHelper.Models;

namespace Ocuda.PolarisHelper
{
    public interface IPolarisHelper
    {
        PatronValidateResult AuthenticatePatron(string barcode, string password);
        Task<string> GetPatronCodeNameAsync(int patronCodeId);
        PatronData GetPatronData(string barcode, string password);
        PatronData GetPatronDataOverride(string barcode);
        RenewRegistrationResult RenewPatronRegistration(string barcode, string email);
    }
}
