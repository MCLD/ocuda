using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.BooksByMail.ViewModels;
using Ocuda.Ops.Controllers.ServiceFacades;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Controllers.Areas.BooksByMail
{
    [Area(nameof(BooksByMail))]
    [Route("[area]/[controller]")]
    public class HomeController : BaseController<HomeController>
    {
        private const int DefaultDays = -21;
        private const string PageTitle = "Books By Mail";

        private readonly IBooksByMailService _booksByMailService;
        private readonly IConfiguration _config;
        private readonly ICustomerLookupService _customerLookupService;

        public HomeController(Controller<HomeController> context,

            IConfiguration config,
            IBooksByMailService booksByMailService,
            ICustomerLookupService customerLookupService) : base(context)
        {
            ArgumentNullException.ThrowIfNull(booksByMailService);
            ArgumentNullException.ThrowIfNull(customerLookupService);
            ArgumentNullException.ThrowIfNull(config);

            _config = config;
            _booksByMailService = booksByMailService;
            _customerLookupService = customerLookupService;

            SetPageTitle(PageTitle);
        }

        public static string Area
        { get { return "BooksByMail"; } }

        public static string Name
        { get { return "Home"; } }

        [HttpPost("[action]")]
        public async Task<JsonResult> AddComment(int id, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return Json(new { success = false, message = "Comment text cannot be empty." });
            }

            string message = string.Empty;
            bool success = false;

            var customer = await _booksByMailService.GetAsync(id);

            if (customer == null)
            {
                _logger.LogError("Could not find customer {CustomerId} for adding comment", id);
                message = $"Unable to find customer id {id}.";
            }

            var comment = new BooksByMailComment
            {
                BooksByMailCustomerId = id,
                Text = text.Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = CurrentUserId
            };

            try
            {
                comment = await _booksByMailService.AddCommentAsync(comment);
                success = true;
            }
            catch (OcudaException oex)
            {
                _logger.LogError(oex, "Error adding comment for customer {CustomerId}: {ErrorMessage}",
                    id,
                    oex.Message);
                message = "An error occured adding your comment.";
            }

            return Json(new
            {
                success,
                message,
                text = comment.Text,
                createdAt = comment.CreatedAt.ToShortDateString()
            });
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> BooksByMailCustomer(int id, string search)
        {
            search = search?.Trim();
            var customerLookup = await _customerLookupService.GetCustomerLookupInfoAsync(id);
            if (customerLookup == null)
            {
                _logger.LogInformation("No customer found for id: {CustomerId}", id);
                ShowAlertWarning("Customer could not be found.");
                return RedirectToAction(nameof(Index));
            }

            var booksByMailCustomer = await _booksByMailService.GetAsync(id);
            if (booksByMailCustomer == null)
            {
                booksByMailCustomer = new Models.Entities.BooksByMailCustomer
                {
                    ExternalCustomerId = id
                };
                booksByMailCustomer = await _booksByMailService.AddAsync(booksByMailCustomer);
            }

            var viewModel = new BooksByMailCustomerViewModel
            {
                BooksByMailCustomer = booksByMailCustomer,
                CustomerLookup = customerLookup,
                CustomerLookupCheckouts = await _customerLookupService
                    .GetCustomerLookupCheckoutsAsync(id),
                CustomerLookupHistoryCount = await _customerLookupService
                    .GetCustomerLookupHistoryCountAsync(id),
                CustomerLookupHolds = await _customerLookupService.GetCustomerLookupHoldsAsync(id),
                Search = search
            };

            if (!string.IsNullOrEmpty(customerLookup.NameLast))
            {
                SetPageTitle($"{PageTitle} - customer '{customerLookup.NameFirst} {customerLookup.NameLast}'");
                viewModel.SecondaryHeading = $"{customerLookup.NameFirst} {customerLookup.NameLast}";
            }

            return View(viewModel);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetCustomerLookupHistory(int customerLookupID,
            string search,
            int orderBy,
            bool orderDesc,
            int page)
        {
            var currentPage = page < 1 ? 1 : page;
            search = search?.Trim();

            var filter = new MaterialFilter(currentPage)
            {
                CustomerLookupID = customerLookupID,
                Search = search,
                OrderBy = (MaterialFilter.OrderType)orderBy,
                OrderDesc = orderDesc
            };

            var itemList = await _customerLookupService
                .GetPaginatedCustomerLookupHistoryAsync(filter);

            var viewModel = new HistoryViewModel
            {
                CurrentPage = currentPage,
                ItemCount = itemList.Count,
                Items = itemList.Data,
                ItemsPerPage = filter.Take.Value,
                OrderBy = orderBy,
                OrderDesc = orderDesc
            };

            if (viewModel.PastMaxPage)
            {
                return RedirectToAction(nameof(GetCustomerLookupHistory), new
                {
                    customerLookupID,
                    search,
                    orderBy,
                    orderDesc,
                    page = viewModel.MaxPage
                });
            }

            return PartialView("_HistoryPartial", viewModel);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Index(string search,
            int orderBy,
            bool orderDesc,
            int page)
        {
            int currentPage = page < 1 ? 1 : page;

            search = search?.Trim();

            var filter = new CustomerLookupFilter(currentPage)
            {
                Search = search,
                OrderBy = (CustomerLookupFilter.OrderType)orderBy,
                OrderDesc = orderDesc
            };

            var customerLookupList = await _customerLookupService
                .GetPaginatedCustomerLookupListAsync(filter);

            var viewModel = new IndexViewModel
            {
                CustomerLookup = customerLookupList.Data,
                ItemCount = customerLookupList.Count,
                CurrentPage = currentPage,
                ItemsPerPage = filter.Take.Value,
                OrderBy = orderBy,
                OrderDesc = orderDesc,
                Search = search,
                SearchCount = customerLookupList.Count
            };

            if (viewModel.PastMaxPage)
            {
                return RedirectToAction(nameof(Index), new
                {
                    search,
                    orderBy,
                    orderDesc,
                    page = viewModel.MaxPage
                });
            }

            // this should move to the view
            foreach (var customerLookup in viewModel.CustomerLookup.Where(_ => _.LastActivityDate.HasValue))
            {
                if (customerLookup.LastActivityDate <= DateTime.Now.AddDays(DefaultDays))
                {
                    customerLookup.LastActivityClass = "text-danger add-alert";
                }
            }

            if (!string.IsNullOrEmpty(search))
            {
                SetPageTitle($"{PageTitle} -  search for '{search}'");
                viewModel.SecondaryHeading = search;
            }

            return View(viewModel);
        }

        [HttpPost("[action]")]
        public async Task<JsonResult> UpdateCustomerField(int id, string field, string text)
        {
            string message = string.Empty;
            bool success = false;

            var customer = await _booksByMailService.GetAsync(id);

            if (customer == null)
            {
                _logger.LogError("Could not find customer {CustomerId} for updating {FieldName}",
                    id,
                    field);
                message = "Could not find customer.";
            }
            else
            {
                switch (field?.ToUpperInvariant())
                {
                    case "NOTES":
                        customer.Notes = text?.Trim();
                        break;

                    case "LIKES":
                        customer.Likes = text?.Trim();
                        break;

                    case "DISLIKES":
                    default:
                        customer.Dislikes = text?.Trim();
                        break;
                }

                try
                {
                    await _booksByMailService.UpdateCustomerAsync(customer);
                    success = true;
                }
                catch (OcudaException oex)
                {
                    _logger.LogError(oex,
                        "Error updating field {FieldName} for customer {CustomerId}: {ErrorMessage}",
                        id,
                        field,
                        oex.Message);
                    message = $"Error updating customer field {field}.";
                }
            }

            return Json(new { success, message, text });
        }
    }
}