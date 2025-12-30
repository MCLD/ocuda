using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface ILdapService
    {
        IEnumerable<string> GetAllLocations();

        IEnumerable<User> GetAllUsers();

        User LookupByEmail(User user);

        User LookupByUsername(User user);

        User VerifyCredentials(string username, string password);
    }
}