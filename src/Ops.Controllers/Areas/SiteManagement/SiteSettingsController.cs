using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.SiteSettings;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement
{
    [Area("SiteManagement")]
    [Authorize(Policy = nameof(ClaimType.SiteManager))]
    [Route("[area]/[controller]")]
    public class SiteSettingsController : BaseController<SiteSettingsController>
    {
        private readonly ISiteSettingPromService _siteSettingPromService;

        public static string Name { get { return "SiteSettings"; } }
        public static string Area { get { return "SiteManagement"; } }

        public SiteSettingsController(ServiceFacades.Controller<SiteSettingsController> context,
            ISiteSettingPromService siteSettingPromService) : base(context)
        {
            _siteSettingPromService = siteSettingPromService 
                ?? throw new ArgumentNullException(nameof(siteSettingPromService));
        }

        [Route("")]
        [Route("[action]")]
        public async Task<IActionResult> Index()
        {
            var siteSettings = await _siteSettingPromService.GetAllAsync();
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
                    if(model == null)
                    {
                        throw new OcudaException("No site setting provided to update.");
                    }

                    var key = model.SiteSetting.Id;
                    var value = model.SiteSetting.Value;
                    var siteSetting = await _siteSettingPromService.UpdateAsync(key, value);
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
