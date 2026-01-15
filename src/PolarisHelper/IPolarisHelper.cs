using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Models;
using Ocuda.PolarisHelper.Models;

namespace Ocuda.PolarisHelper
{
    public interface IPolarisHelper
    {
        bool IsConfigured { get; }
        bool AuthenticateCustomer(string barcode, string password);
        Task<List<CustomerBlock>> GetCustomerBlocksAsync(int customerId);
        Task<string> GetCustomerCodeNameAsync(int customerCodeId);
        Customer GetCustomerData(string barcode, string password);
        Customer GetCustomerDataOverride(string barcode);
        RenewRegistrationResult RenewCustomerRegistration(string barcode, string email);
    }
}
