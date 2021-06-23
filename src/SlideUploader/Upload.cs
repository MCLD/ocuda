using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models;
using Ocuda.Utility.Exceptions;

namespace Ocuda.SlideUploader
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance",
        "CA1812:Avoid uninstantiated internal classes",
        Justification = "Class is instantiated via dependency injection")]
    internal class Upload
    {
        private readonly ILogger<Upload> _logger;
        private readonly OpsClient _opsClient;

        public Upload(ILogger<Upload> logger,
            OpsClient opsClient)
        {
            _opsClient = opsClient ?? throw new ArgumentNullException(nameof(opsClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        internal async Task JobAsync(string jobFile, string jobResultFile)
        {
            var jobFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, jobFile);
            var jobResultFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                jobResultFile);

            if (!System.IO.File.Exists(jobFilePath))
            {
                _logger.LogCritical("No {JobFile} job file found.", jobFile);
                await JobFailureAsync(jobResultFilePath, $"No {jobFile} job file found.");
                return;
            }

            var checkAuth = await _opsClient.IsAuthenticatedAsync();

            if (string.IsNullOrEmpty(checkAuth))
            {
                _logger.LogCritical("Unable to continue upload, cannot authenticate");
                await JobFailureAsync(jobResultFilePath, "Unable to upload: cannot authenticate.");
                return;
            }

            SlideUploadJob job;

            try
            {
                job = ParseJobFile(jobFilePath);
            }
            catch (OcudaException oex)
            {
                var ex = oex.InnerException ?? oex;
                _logger.LogCritical(ex,
                    "Unable read job file {JobFile}: {ErrorMessage}",
                    jobFile,
                    ex.Message);
                await JobFailureAsync(jobResultFilePath, $"Critical job error: {ex.Message}.");
                return;
            }

            var jobResult = await _opsClient.UploadJobAsync(job);

            if (jobResult.Success)
            {
                _logger.LogInformation("Uploaded {FileName} as {Username} to set {Set}: {Message}",
                    Path.GetFileName(job.Filepath),
                    checkAuth,
                    job.Set,
                    jobResult.Message);
            }
            else
            {
                _logger.LogError("Failure {ServerResponse} uploading {FileName} as {Username} to set {Set}: {Message}",
                    jobResult.ServerResponse ? "from server" : "without server response",
                    Path.GetFileName(job.Filepath),
                    checkAuth,
                    job.Set,
                    jobResult.Message);
            }

            await System.IO.File.WriteAllTextAsync(jobResultFilePath,
                JsonSerializer.Serialize(jobResult));
        }

        private static async Task JobFailureAsync(string jobResultFilePath, string message)
        {
            await File.WriteAllTextAsync(jobResultFilePath,
                JsonSerializer.Serialize(new JsonResponse
                {
                    Success = false,
                    Message = message
                }));
        }

        private static SlideUploadJob ParseJobFile(string jobFile)
        {
            string jobText;

            try
            {
                jobText = System.IO.File.ReadAllText(jobFile);
            }
            catch (IOException ioex)
            {
                throw new OcudaException("Unable to read text from file", ioex);
            }

            if (string.IsNullOrEmpty(jobText))
            {
                throw new OcudaException("Job file is empty");
            }

            SlideUploadJob job;

            try
            {
                job = JsonSerializer.Deserialize<SlideUploadJob>(jobText);
                ValidateJob(job);
            }
            catch (JsonException jex)
            {
                throw new OcudaException("Unable to parse job file", jex);
            }

            return job;
        }

        private static void ValidateJob(SlideUploadJob job)
        {
            if (job == null)
            {
                throw new OcudaException("Could not create job object from file");
            }

            if (string.IsNullOrEmpty(job.Filepath))
            {
                throw new OcudaException("File name not specified in job file");
            }

            if (job.StartDate == default)
            {
                throw new OcudaException("Start date not specified in job file");
            }

            if (job.EndDate == default)
            {
                throw new OcudaException("End date not specified in job file");
            }

            if (string.IsNullOrEmpty(job.Set))
            {
                throw new OcudaException("Set not specified in job file");
            }
        }
    }
}