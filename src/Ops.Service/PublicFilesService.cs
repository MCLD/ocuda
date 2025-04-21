using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Ops.Services;

namespace Ocuda.Ops.Service
{
    public class PublicFilesService : BaseService<PublicFilesService>, IPublicFilesService
    {
        private readonly IConfiguration _config;

        public PublicFilesService(ILogger<PublicFilesService> logger,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration config) : base(logger, httpContextAccessor)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            // Ops.PublicSiteUrlSharedContent
        }


    }
}
