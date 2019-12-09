﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.SiteSettings;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement
{
    [Area("SiteManagement")]
    [Authorize(Policy = nameof(ClaimType.SiteManager))]
    [Route("[area]/[controller]")]
    public class SiteSettingsController : BaseController<SiteSettingsController>
    {
        public SiteSettingsController(ServiceFacades.Controller<SiteSettingsController> context)
            : base(context)
        {
        }

        [Route("")]
        [Route("[action]")]
        public async Task<IActionResult> Index()
        {
            var siteSettings = await _siteSettingService.GetAllAsync();
            var categories = siteSettings.Select(_ => _.Category).Distinct();
            var siteSettingsByCategory = new Dictionary<string, List<SiteSetting>>();

            foreach (var category in categories)
            {
                var settings = siteSettings.Where(_ => _.Category == category).ToList();
                siteSettingsByCategory.Add(category, settings);
            }

            var viewModel = new IndexViewModel
            {
                SiteSettingsByCategory = siteSettingsByCategory
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Update(IndexViewModel model)
        {
            ModelState.Remove("SiteSetting.Description");
            ModelState.Remove("SiteSetting.Name");

            if (ModelState.IsValid)
            {
                try
                {
                    var key = model.SiteSetting.Key;
                    var value = model.SiteSetting.Value;
                    var siteSetting = await _siteSettingService.UpdateAsync(key, value);
                    ShowAlertSuccess($"Updated {siteSetting.Name}");
                }
                catch (OcudaException ex)
                {
                    ShowAlertDanger("Unable to update site settings: ", ex.Message);
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
