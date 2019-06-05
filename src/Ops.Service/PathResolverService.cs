using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Ops.Services;

namespace Ocuda.Ops.Service
{
    public class PathResolverService : IPathResolverService
    {
        private const string DefaultPublicDirectory = "public";
        private const string DefaultPrivateDirectory = "private";

        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        public PathResolverService(ILogger<PathResolverService> logger,
            IConfiguration config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public string GetPublicContentUrl(params object[] pathElement)
        {
            var path = new StringBuilder(_config[Utility.Keys.Configuration.OpsUrlSharedContent]);
            if (path.Length == 0)
            {
                path.Append("content");
            }
            foreach (var element in pathElement)
            {
                if (!path.ToString().EndsWith("/") && !element.ToString().StartsWith("/"))
                {
                    path.Append("/");
                }
                path.Append(element);
            }
            return path.ToString();
        }

        public string GetPublicContentFilePath(string fileName = default(string),
            params object[] pathElement)
        {
            return GetContentFilePath(false, fileName, pathElement);
        }

        public string GetPrivateContentFilePath(string fileName = default(string),
            params object[] pathElement)
        {
            return GetContentFilePath(true, fileName, pathElement);
        }
        private string GetContentFilePath(bool privatePath = false,
            string fileName = default(string),
            params object[] pathElement)
        {
            string publicPrivateRoot = privatePath
                ? DefaultPrivateDirectory
                : DefaultPublicDirectory;

            string path
                = Utility.Files.SharedPath.Get(_config[Utility.Keys.Configuration.OpsFileShared]);

            try
            {
                path = Path.Combine(path, publicPrivateRoot);
                Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to create {0} file directory {1}: {2}",
                    publicPrivateRoot, path, ex.Message);
                throw;
            }

            foreach (var element in pathElement)
            {
                path = Path.Combine(path, element.ToString());

                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unable to create {0} file directory {1}: {2}",
                        publicPrivateRoot, path, ex.Message);
                    throw;
                }
            }

            if (!string.IsNullOrEmpty(fileName))
            {
                path = Path.Combine(path, fileName);
            }

            return path;
        }
    }
}
