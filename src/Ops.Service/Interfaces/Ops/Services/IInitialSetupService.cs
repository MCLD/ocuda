using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IInitialSetupService
    {
        Task VerifyInitialSetupAsync();
    }
}
