using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface ILdapService
    {
        User LookupByEmail(User user);
        User LookupByUsername(User user);
    }
}
