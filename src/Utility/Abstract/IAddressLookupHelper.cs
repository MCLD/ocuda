using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Models;

namespace Ocuda.Utility.Abstract
{
    public interface IAddressLookupHelper
    {
        Task<IEnumerable<AddressAssociation>> GetAssociatedEntitiesAsync(string address, string zip);
    }
}