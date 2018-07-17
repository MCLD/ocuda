using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IFileTypeService
    {
        Task<FileType> GetByExtensionAsync(string extension);
    }
}
