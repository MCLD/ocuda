using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Service;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Promenade.Web
{
    public class Web
    {
        private readonly ILogger<Web> _log;
        private readonly IServiceProvider _serviceProvider;

        public Web(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider
                ?? throw new ArgumentNullException(nameof(serviceProvider));
            _log = serviceProvider.GetRequiredService<ILogger<Web>>();
        }

        public async Task InitalizeAsync()
        {
            using var serviceScope = _serviceProvider.CreateScope();
            try
            {
                var languageService
                    = serviceScope.ServiceProvider.GetRequiredService<LanguageService>();
                await languageService.SyncLanguagesAsync();
            }
            catch (InvalidOperationException ioex)
            {
                _log.LogCritical(ioex,
                    "Error acquiring LanguageService to sync languages in database: {ErrorMessage}",
                    ioex.Message);
            }
            catch (Exception ex)
            {
                _log.LogCritical(ex,
                    "Error syncing available languages with database: {ErrorMessage}",
                    ex.Message);
                throw;
            }

            string currentFile = null;
            try
            {
                var pathResolverService
                    = serviceScope.ServiceProvider.GetRequiredService<IPathResolverService>();

                var customRootPath = pathResolverService.GetPrivateContentFilePath("wwwroot");

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