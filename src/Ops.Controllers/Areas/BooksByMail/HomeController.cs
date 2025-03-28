using System;
using System.Linq;
using System.Threading.Tasks;
using Ocuda.Utility.Models;
using Ocuda.Ops.Controllers.Areas.BooksByMail.ViewModels.Home;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Controllers.ServiceFacades;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Controllers.Areas.BooksByMail
{
    [Area("BooksByMail")]
    [Route("BooksByMail/[controller]/[action]")]
    public class HomeController : BaseController<HomeController>
    {
        private const int DefaultDays = -21;
        private const string PageTitle = "Books By Mail";

        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _config;
        private readonly IBooksByMailService _customerService;
        private readonly ICustomerService _polarisService;
        public HomeController(Controller<HomeController> context,
            ILogger<HomeController> logger,
            IConfiguration config,
            IBooksByMailService customerService,
            ICustomerService polarisService) : base(context)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _customerService = customerService
                ?? throw new ArgumentNullException(nameof(customerService));
            _polarisService = polarisService
                ?? throw new ArgumentNullException(nameof(polarisService));
        }

        public static string Name
        { get { return "BooksByMail"; } }

        public async Task<IActionResult> Index(string search, int orderBy, bool orderDesc,
            int page = 1)
        {
            search = search?.Trim();
            var filter = new PolarisPatronFilter(page)
            {
                Search = search,
                OrderBy = (PolarisPatronFilter.OrderType)orderBy,
                OrderDesc = orderDesc
            };

            var patronList = await _polarisService.GetPaginatedPatronListAsync(filter);

            int days = DefaultDays;
            foreach (var patron in patronList.Data.Where(_ => _.LastActivityDate != null))
            {
                if (patron.LastActivityDate <= DateTime.Now.AddDays(days))
                {
                    patron.LastActivityClass = "text-danger add-alert";
                }
            }

            var paginateModel = new PaginateModel
            {
                ItemCount = patronList.Count,
                CurrentPage = page,
                ItemsPerPage = filter.Take.Value
            };

            if (paginateModel.MaxPage > 0 && paginateModel.CurrentPage > paginateModel.MaxPage)
            {
                return RedirectToRoute(
                    new
                    {
                        page = paginateModel.LastPage ?? 1
                    });
            }

            var viewModel = new IndexViewModel
            {
                Patrons = patronList.Data,
                PaginateModel = paginateModel,
                OrderBy = orderBy,
                OrderDesc = orderDesc,
                Search = search,
                SearchCount = patronList.Count
            };

            ViewData["Search"] = search;


            ViewData["Title"] = string.IsNullOrEmpty(search)
                ? "Books By Mail"
                : $"Books By Mail - search for '{search}'";

            return View(viewModel);
        }

        public async Task<IActionResult> BooksByMailCustomer(int id)
        {
            var patronInfo = await _polarisService.GetPatronInfoAsync(id);
            if (patronInfo == null)
            {
                _logger.LogInformation($"No patron found in Polaris for id {id}");
                ShowAlertWarning("Patron could not be found.");
                return RedirectToAction(nameof(Index));
            }

            var booksbymailcustomer = await _customerService.GetByPatronIdAsync(id);
            if (booksbymailcustomer == null)
            {
                booksbymailcustomer = new Models.Entities.BooksByMailCustomer
                {
                    PatronID = id
                };
                booksbymailcustomer = await _customerService.AddAsync(booksbymailcustomer);
            }

            var viewModel = new ViewModels.Home.BooksByMailCustomerViewModel
            {
                BooksByMailCustomer = booksbymailcustomer,
                Patron = patronInfo,
                PatronCheckouts = await _polarisService.GetPatronCheckoutsAsync(id),
                PatronHolds = await _polarisService.GetPatronHoldsAsync(id),
                PatronHistoryCount = await _polarisService.GetPatronHistoryCountAsync(id)
            };

            ViewData["Title"] = string.IsNullOrEmpty(patronInfo.NameLast)
                ? "Books By Mail"
                : $"Books By Mail - customer '{patronInfo.NameFirst} {patronInfo.NameLast}'";

            return View(viewModel);
        }

        public async Task<IActionResult> GetPatronHistory(int patronID, string search, int orderBy,
            bool orderDesc, int page = 1)
        {
            search = search?.Trim();
            var filter = new PolarisItemFilter(page)
            {
                PatronID = patronID,
                Search = search,
                OrderBy = (PolarisItemFilter.OrderType)orderBy,
                OrderDesc = orderDesc
            };

            var itemList = await _polarisService.GetPaginatedPatronHistoryAsync(filter);

            var paginateModel = new PaginateModel
            {
                ItemCount = itemList.Count,
                CurrentPage = page,
                ItemsPerPage = filter.Take.Value
            };

            var viewModel = new HistoryViewModel
            {
                Items = itemList.Data,
                PaginateModel = paginateModel,
                OrderBy = orderBy,
                OrderDesc = orderDesc
            };

            return PartialView("_HistoryPartial", viewModel);
        }

        [HttpPost]
        public async Task<JsonResult> UpdateCustomerField(int id, string field, string text)
        {
            string message = string.Empty;
            bool success = false;

            var customer = await _customerService.GetByIdAsync(id);

            if (customer == null)
            {
                _logger.LogError($"Could not find customer {id} for updating {field}");
                message = $"Could not find customer.";
            }
            else
            {

                switch (field.ToLower())
                {
                    case "notes":
                        customer.Notes = text?.Trim();
                        break;
                    case "likes":
                        customer.Likes = text?.Trim();
                        break;
                    case "dislikes":
                        customer.Dislikes = text?.Trim();
                        break;
                    default:
                        break;
                }

                try
                {
                    await _customerService.UpdateAsync(customer);
                    success = true;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error updating field {field} for customer {id}", ex);
                    message = $"Error updating customer {field}.";
                }
            }

            return Json(new { success, message, text });
        }

        [HttpPost]
        public async Task<JsonResult> AddComment(int id, string text)
        {

            if (string.IsNullOrWhiteSpace(text))
            {
                return Json(new { success = false, message = "Comment text cannot be empty." });
            }

            string message = string.Empty;
            bool success = false;

            var customer = await _customerService.GetByIdAsync(id);

            if (customer == null)
            {
                _logger.LogError($"Could not find customer {id} for adding comment");
                message = $"Could not find customer.";
            }

            var comment = new BooksByMailComment
            {
                CreatedAt = DateTime.Now,
                CustomerId = id,
                StaffUsername = HttpContext.User.Identity.Name,
                Text = text.Trim()
            };

            try
            {
                comment = await _customerService.AddCommentAsync(comment);
                success = true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding comment for customer {id}", ex);
                message = $"Error adding comment.";
            }

            return Json(new
            {
                success,
                message,
                text = comment.Text,
                username = comment.StaffUsername,
                createdAt = comment.CreatedAt.ToShortDateString()
            });
        }
    }
}
