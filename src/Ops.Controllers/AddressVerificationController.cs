using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using MaricopaCountyAssessorHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Ocuda.Models;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.ServiceFacades;
using Ocuda.Ops.Models;
using Ocuda.PolarisHelper;
using Ocuda.Utility.Exceptions;
using TrestleHelper;

namespace Ocuda.Ops.Controllers
{
    [Route("[controller]")]
    public class AddressVerificationController(Controller<AddressVerificationController> context,
        IOptions<OpsFeaturesOptions> features,
        IPolarisHelper polarisHelper,
        MaricopaCountyAssessorClient maricopaCountyAssessorClient,
        TrestleClient trestleClient) : BaseController<AddressVerificationController>(context)
    {
        private const string PageTitle = "Address Verificaiton";

        public static string Name
        { get { return "AddressVerification"; } }

        [HttpGet("[action]")]
        public IActionResult BarcodeLookup(string barcode)
        {
            var response = new JsonResponse<IEnumerable<AddressAssociation>>();
            try
            {
                var customerData = polarisHelper.GetCustomerDataOverride(barcode);
                if (customerData == null || customerData.Addresses == null)
                {
                    response.ServerResponse = false;
                    response.Success = false;
                    response.Message = "No customer addresses found.";
                }
                else
                {
                    response.ServerResponse = true;
                    var data = new List<AddressAssociation>();
                    foreach (var address in customerData.Addresses)
                    {
                        data.Add(new AddressAssociation
                        {
                            Entities = [$"{customerData.NameFirst} {customerData.NameLast}"],
                            PostalCode = address.PostalCode,
                            PropertyType = address.AddressType,
                            StreetAddress1 = address.StreetAddressOne
                        });
                    }
                    response.Data = data;
                    response.Success = true;
                }
            }
            catch (OcudaException oex)
            {
                response.ServerResponse = false;
                response.Success = false;
                response.Message = $"An error occurred: {oex.Message}";
            }

            return Json(response);
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var viewModel = new ViewModels.AddressVerification.IndexViewModel
            {
                AddressLookupConfigured = features.Value.AddressLookupConfigured,
                BarcodeLookupConfigured = features.Value.PolarisHelperConfigured
            };

            if (!viewModel.AddressLookupConfigured && !viewModel.BarcodeLookupConfigured)
            {
                ShowAlertDanger("Address verification and barcode lookup are not configured.");
                RedirectToAction(nameof(HomeController.Index), HomeController.Name);
            }

            viewModel.Link = await _siteSettingService
                .GetSettingStringAsync(Models.Keys.SiteSetting.AddressVerification.Link);

            if (!string.IsNullOrEmpty(viewModel.Link))
            {
                viewModel.LinkText = await _siteSettingService
                    .GetSettingStringAsync(Models.Keys.SiteSetting.AddressVerification.LinkText)
                    ?? "Information";
            }

            SetPageTitle(PageTitle);

            return View(viewModel);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> MaricopaCountyAssessorLookup(string address, string zip)
        {
            var jsonResponse = new JsonResponse<IEnumerable<AddressAssociation>>
            {
                Source = MaricopaCountyAssessorClient.Source
            };

            try
            {
                jsonResponse.Data = await maricopaCountyAssessorClient
                    .GetAssociatedEntitiesAsync(address, zip);
                jsonResponse.ServerResponse = true;
                jsonResponse.Success = true;
            }
            catch (OcudaException oex)
            {
                jsonResponse.ServerResponse = false;
                jsonResponse.Success = false;
                jsonResponse.Message = oex.Message;
            }

            return Json(jsonResponse);
        }

        [HttpGet("[action]")]
        public IActionResult Providers()
        {
            var providers = new Dictionary<string, dynamic>();

            if (features.Value.AddressLookupMaricopaCountyConfigured)
            {
                providers.Add(MaricopaCountyAssessorClient.Source,
                    new
                    {
                        Name = "Maricopa County Assessor",
                        Path = Url.Action(nameof(MaricopaCountyAssessorLookup)),
                        MaricopaCountyAssessorClient.Source
                    });
            }
            if (features.Value.AddressLookupTrestleConfigured)
            {
                providers.Add(TrestleClient.Source,
                    new
                    {
                        Name = "Trestle",
                        Path = Url.Action(nameof(TrestleLookup)),
                        TrestleClient.Source
                    });
            }

            return Content($"const lookupProviders = {JsonSerializer.Serialize(providers)};",
                "text/javascript");
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> TrestleLookup(string address, string zip)
        {
            var jsonResponse = new JsonResponse<IEnumerable<AddressAssociation>>
            {
                Source = TrestleClient.Source
            };

            try
            {
                jsonResponse.Data = await trestleClient
                    .GetAssociatedEntitiesAsync(address, zip);
                jsonResponse.ServerResponse = true;
                jsonResponse.Success = true;
            }
            catch (OcudaException oex)
            {
                jsonResponse.ServerResponse = false;
                jsonResponse.Success = false;
                jsonResponse.Message = oex.Message;
            }

            return Json(jsonResponse);
        }
    }
}