using System;
using System.Collections.Generic;
using System.Text;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IPathResolverService
    {
        string GetPublicContentUrl(params object[] pathElement);
        string GetPublicContentFilePath(string fileName = default(string), params object[] pathElement);
        string GetPrivateContentFilePath(string fileName = default(string), params object[] pathElement);
    }
}
