﻿using System;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ocuda.Utility.Services
{
    public class PathResolverService : Interfaces.IPathResolverService
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
            var path = new StringBuilder(_config[Keys.Configuration.OcudaUrlSharedContent]);
            if (path.Length == 0)
            {
                path.Append("content");
            }
            if (pathElement != null)
            {
                foreach (var element in pathElement)
                {
                    if (!path.ToString().EndsWith("/", StringComparison.OrdinalIgnoreCase)
                        && !element.ToString().StartsWith("/", StringComparison.OrdinalIgnoreCase))
                    {
                        path.Append("/");
                    }
                    path.Append(element);
                }
            }
            return path.ToString();
        }

        public string GetPublicContentFilePath(string fileName = default,
            params object[] pathElement)
        {
            return GetContentFilePath(false, fileName, pathElement);
        }

        public string GetPrivateContentFilePath(string fileName = default,
            params object[] pathElement)
        {
            return GetContentFilePath(true, fileName, pathElement);
        }

        private string GetContentFilePath(bool privatePath = false,
            string fileName = default,
            params object[] pathElement)
        {
            string publicPrivateRoot = privatePath
                ? DefaultPrivateDirectory
                : DefaultPublicDirectory;

            string path = Files.SharedPath.Get(_config[Keys.Configuration.OcudaFileShared]);

            try
            {
                path = System.IO.Path.Combine(path, publicPrivateRoot);
                System.IO.Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unable to create {PublicOrPrivate} file directory {FilePath}: {Message}",
                    publicPrivateRoot,
                    path,
                    ex.Message);
                throw;
            }

            foreach (var element in pathElement)
            {
                path = System.IO.Path.Combine(path, element.ToString());

                try
                {
                    System.IO.Directory.CreateDirectory(path);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Unable to create {PublicOrPrivate} file directory {FilePath}: {Message}",
                        publicPrivateRoot,
                        path,
                        ex.Message);
                    throw;
                }
            }

            if (!string.IsNullOrEmpty(fileName))
            {
                path = System.IO.Path.Combine(path, fileName);
            }

            return path;
        }
    }
}
