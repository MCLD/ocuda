using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Roster;
using Ocuda.Ops.Service;

namespace Ocuda.Ops.Controllers.Areas.Admin
{
    [Area("Admin")]
    public class RosterController : BaseController
    {
        private RosterService _rosterService;
        private ILogger<RosterController> _logger;

        public RosterController(ILogger<RosterController> logger,
            ServiceFacade.Controller context,
            RosterService rosterService) : base(context)
        {
            _rosterService = rosterService
                ?? throw new ArgumentNullException(nameof(rosterService));
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Changes()
        {
            var rosterChanges = await _rosterService.GetRosterChangesAsync();

            var viewModel = new ChangesViewModel
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
            catch (Exception ex)
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
        public async Task<IActionResult> Upload(UploadViewModel model)
        {
            if (ModelState.IsValid)
            {
                var tempFile = Path.GetTempFileName();
                using (var fileStream = new FileStream(tempFile, FileMode.Create))
                {
                    await model.Roster.CopyToAsync(fileStream);
                }

                try
                {
                    int insertedRecordCount
                        = await _rosterService.UploadRosterAsync(CurrentUserId, tempFile);
                    AlertInfo = $"Successfully inserted {insertedRecordCount} roster records.";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error inserting roster data: {Message}", ex);
                    AlertDanger = $"There was a problem inserting the roster data: {ex.Message}";
                }

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
