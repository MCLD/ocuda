using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.ViewModels.Roster;
using Ocuda.Ops.Service;

namespace Ocuda.Ops.Controllers
{
    public class RosterController : Abstract.BaseController
    {
        private RosterService _rosterService;
        private ILogger<RosterController> _logger;

        public RosterController(RosterService rosterService, ILogger<RosterController> logger)
        {
            _rosterService = rosterService
                ?? throw new ArgumentNullException(nameof(rosterService));
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Changes()
        {
            var rosterChanges = await _rosterService.GetRosterChangesAsync();

            var viewModel = new RosterChangesViewModel
            {
                RosterDetail = rosterChanges.RosterDetail,
                NewEmployees = rosterChanges.NewEmployees,
                RemovedEmployees = rosterChanges.RemovedEmployees
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveChange(int rosterId)
        {
            var result = false;
            var message = string.Empty;

            try
            {
                result = await _rosterService.ApproveRosterChanges(rosterId);
            }
            catch(Exception ex)
            {
                _logger.LogError("Error approving roster entry: ", ex);
                message = "An error occured while trying to approve the change: " + ex.Message;
            }
            return Json(new
            {
                success = result,
                message = message
            });
        }

        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(RosterUploadViewModel model)
        {
            if (ModelState.IsValid)
            {
                var tempFile = Path.GetTempFileName();
                using (var fileStream = new FileStream(tempFile, FileMode.Create))
                {
                    await model.Roster.CopyToAsync(fileStream);
                }
                await _rosterService.UploadRosterAsync(tempFile);

                return RedirectToAction(nameof(Upload));
            }
            else
            {
                var filenameErrors = ModelState[nameof(model.FileName)]?.Errors?.ToList();
                if (filenameErrors?.Count > 0)
                {
                    foreach (var error in filenameErrors)
                    {
                        ModelState[nameof(model.Roster)].Errors.Add(error);
                    }
                }
            }

            return View(model);
        }
    }
}
