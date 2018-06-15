using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.ViewModels.Roster;
using Ops.Service;

namespace Ocuda.Ops.Controllers
{
    public class RosterController : Abstract.BaseController
    {
        private RosterService _rosterService;
        public RosterController(RosterService rosterService)
        {
            _rosterService = rosterService
                ?? throw new ArgumentNullException(nameof(rosterService));
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
