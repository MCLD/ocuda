using System;
using System.Threading.Tasks;
using Ocuda.Ops.Service;

namespace Ocuda.Ops.Controllers.Validator
{
    public class SectionPathValidator : ISectionPathValidator
    {
        private readonly SectionService _sectionService;

        public SectionPathValidator(SectionService sectionService)
        {
            _sectionService = sectionService
                ?? throw new ArgumentNullException(nameof(sectionService));
        }

        public bool IsValid(string sectionPath)
        {
            try
            {
                return Task.Run(() => _sectionService.IsValidPathAsync(sectionPath)).Result;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
