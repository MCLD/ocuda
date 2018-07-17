using System;
using System.Threading.Tasks;
using Ocuda.Ops.Service;
using Ocuda.Ops.Service.Interfaces.Ops.Services;

namespace Ocuda.Ops.Controllers.Validator
{
    public class SectionPathValidator : ISectionPathValidator
    {
        private readonly ISectionService _sectionService;

        public SectionPathValidator(ISectionService sectionService)
        {
            _sectionService = sectionService
                ?? throw new ArgumentNullException(nameof(sectionService));
        }

        public bool IsValid(string sectionPath)
        {
            return Task.Run(() => _sectionService.IsValidPathAsync(sectionPath)).Result;
        }
    }
}
