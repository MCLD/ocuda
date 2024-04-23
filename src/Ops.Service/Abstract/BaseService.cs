using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Service.Abstract
{
    public abstract class BaseService<TService> : Utility.Services.OcudaBaseService<TService>
    {
        protected const string ImagesFilePath = "images";
        private readonly IHttpContextAccessor _httpContextAccessor;

        protected BaseService(ILogger<TService> logger,
            IHttpContextAccessor httpContextAccessor)
            : base(logger)
        {
            _httpContextAccessor = httpContextAccessor
                ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        protected int GetCurrentUserId()
        {
            var claim = _httpContextAccessor.HttpContext
                .User
                .Claims
                .First(_ => _.Type == ClaimType.UserId);

            return int.Parse(claim.Value, CultureInfo.InvariantCulture);
        }

        protected async Task<string> GetFullImageDirectoryPath(ISiteSettingService siteSettingService,
            string languageName,
            string subDirectory)
        {
            ArgumentNullException.ThrowIfNull(siteSettingService);

            string basePath = await siteSettingService.GetSettingStringAsync(
                Ops.Models.Keys.SiteSetting.SiteManagement.PromenadePublicPath);

            var filePath = Path.Combine(basePath,
                ImagesFilePath,
                languageName,
                subDirectory);

            if (!Directory.Exists(filePath))
            {
                _logger.LogInformation("Creating image directory for {SubDirectory}: {Path}",
                    subDirectory,
                    filePath);
                Directory.CreateDirectory(filePath);
            }

            return filePath;
        }

        protected IEnumerable<int> GetPermissionIds()
        {
            return _httpContextAccessor.HttpContext
                .User
                .Claims
                .Where(_ => _.Type == ClaimType.PermissionId)
                .Select(_ => int.Parse(_.Value, CultureInfo.InvariantCulture));
        }

        protected bool IsSiteManager()
        {
            return _httpContextAccessor.HttpContext
                .User
                .Claims
                .Any(_ => _.Type == ClaimType.SiteManager);
        }
    }
}