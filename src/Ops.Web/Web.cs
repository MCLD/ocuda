using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Ops.Web
{
    public class Web
    {
        private readonly ILogger<Web> _log;
        private readonly IServiceProvider _serviceProvider;

        public Web(IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);

            _serviceProvider = serviceProvider;
            _log = serviceProvider.GetRequiredService<ILogger<Web>>();
        }

        public void Initalize()
        {
            using var serviceScope = _serviceProvider.CreateScope();

            string currentFile = null;
            try
            {
                var pathResolverService
                    = serviceScope.ServiceProvider.GetRequiredService<IPathResolverService>();

                var customRootPath = pathResolverService.GetPublicContentFilePath("wwwroot");

                if (Directory.Exists(customRootPath))
                {
                    var webHostEnvironment
                        = serviceScope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
                    var wwwroot = webHostEnvironment.WebRootPath;

                    var copiedFiles = new List<string>();

                    foreach (var customFile in Directory.EnumerateFiles(customRootPath, "*.*"))
                    {
                        currentFile = Path.GetFileName(customFile);
                        copiedFiles.Add(currentFile);
                        File.Copy(customFile,
                            Path.Combine(wwwroot, Path.GetFileName(customFile)),
                            true);
                    }
                    if (copiedFiles.Count > 0)
                    {
                        _log.LogInformation("Copied {Count} custom files from {SourcePath} to {DestinationPath}: {Files}",
                            copiedFiles.Count,
                            customRootPath,
                            wwwroot,
                            string.Join(", ", copiedFiles));
                    }
                }
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(currentFile))
                {
                    _log.LogCritical(ex,
                        "Error syncing wwwroot files on file {File}: {ErrorMessage}",
                         currentFile,
                         ex.Message);
                }
                else
                {
                    _log.LogCritical(ex,
                        "Error syncing wwwroot files: {ErrorMessage}",
                        ex.Message);
                }
                throw;
            }
        }
    }
}