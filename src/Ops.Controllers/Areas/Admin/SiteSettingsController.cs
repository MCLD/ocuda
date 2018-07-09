using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Authorization;
using Ocuda.Ops.Service;
using Ocuda.Ops.Controllers.Areas.Admin.ViewModels.SiteSettings;
using System.Linq;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Controllers.Areas.Admin
{
    [Area("Admin")]
    [Authorize(Policy = nameof(SiteManagerRequirement))]
    public class SiteSettingsController : BaseController
    {
        private SiteSettingService _siteSettingService;

        public SiteSettingsController(SiteSettingService siteSettingService)
        {
            _siteSettingService = siteSettingService
                ?? throw new ArgumentNullException(nameof(siteSettingService));
        }

        public async Task<IActionResult> Index()
        {
            var siteSettings = await _siteSettingService.GetAllAsync();
            var categories = siteSettings.Select(_ => _.Category).Distinct();
            var siteSettingsByCategory = new Dictionary<string, List<SiteSetting>>();

            foreach(var category in categories)
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
        public async Task<IActionResult> Update(IndexViewModel model)
        {
            switch (model.SiteSetting.Type)
            {
                case "bool":
                    model.SiteSetting.Value = model.ValueBool;
                    break;
                case "int":
                    model.SiteSetting.Value = model.ValueInt;
                    break;
                default:
                    model.SiteSetting.Value = model.ValueString;
                    break;
            }

            if(ModelState.IsValid)
            {
                try
                {
                    var key = model.SiteSetting.Key;
                    var value = model.SiteSetting.Value;
                    var siteSetting = await _siteSettingService.UpdateAsync(key, value);
                    ShowAlertSuccess($"Updated {siteSetting.Name}");
                }
                catch(Exception ex)
                {
                    ShowAlertDanger("Unable to update site settings: ", ex.Message);
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
