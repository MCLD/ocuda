using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.BooksByMail.ViewModels.Home;
using Ocuda.Ops.Controllers.ServiceFacades;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.BooksByMail
{
    [Area("BooksByMail")]
    [Route("[area]/[controller]")]
    public class HomeController : BaseController<HomeController>
    {
        private const int DefaultDays = -21;
        private const string PageTitle = "Books By Mail";

        private readonly IConfiguration _config;
        private readonly IBooksByMailService _booksByMailService;
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
        }

        public static string Name
        { get { return "Home"; } }

        public static string Area
        { get { return "BooksByMail"; } }

        [HttpGet("[action]")]
        public async Task<IActionResult> Index(string search, int orderBy, bool orderDesc,
            int page)
        {
            int currentPage = page != 0 ? page : 1;

            search = search?.Trim();
            var filter = new CustomerLookupFilter(currentPage)
            {
                Search = search,
                OrderBy = (CustomerLookupFilter.OrderType)orderBy,
                OrderDesc = orderDesc
            };

            var customerLookupList = await _customerLookupService.GetPaginatedCustomerLookupListAsync(filter);

            int days = DefaultDays;
            foreach (var customerLookup in customerLookupList.Data.Where(_ => _.LastActivityDate != null))
            {
                if (customerLookup.LastActivityDate <= DateTime.Now.AddDays(days))
                {
                    customerLookup.LastActivityClass = "text-danger add-alert";
                }
            }

            ViewData["Title"] = string.IsNullOrEmpty(search)
                ? "Books By Mail"
                : $"Books By Mail - search for '{search}'";

            return View(new IndexViewModel
            {
                CustomerLookup = customerLookupList.Data,
                ItemCount = customerLookupList.Count,
                CurrentPage = currentPage,
                ItemsPerPage = filter.Take.Value,
                OrderBy = orderBy,
                OrderDesc = orderDesc,
                Search = search,
                SearchCount = customerLookupList.Count
            });
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> BooksByMailCustomer(int id, string search)
        {
            search = search?.Trim();
            var customerLookup = await _customerLookupService.GetCustomerLookupInfoAsync(id);
            if (customerLookup == null)
            {
                _logger.LogInformation("No customer found in Polaris for id {id}", id);
                ShowAlertWarning("Customer could not be found.");
                return RedirectToAction(nameof(Index));
            }

            var booksByMailCustomer = await _booksByMailService.GetByCustomerLookupIdAsync(id);
            if (booksByMailCustomer == null)
            {
                booksByMailCustomer = new Models.Entities.BooksByMailCustomer
                {
                    CustomerLookupID = id
                };
                booksByMailCustomer = await _booksByMailService.AddAsync(booksByMailCustomer);
            }

            ViewData["Title"] = string.IsNullOrEmpty(customerLookup.NameLast)
                ? "Books By Mail"
                : $"Books By Mail - customer '{customerLookup.NameFirst} {customerLookup.NameLast}'";

            return View(new BooksByMailCustomerViewModel
            {
                BooksByMailCustomer = booksByMailCustomer,
                CustomerLookup = customerLookup,
                CustomerLookupCheckouts = await _customerLookupService.GetCustomerLookupCheckoutsAsync(id),
                CustomerLookupHolds = await _customerLookupService.GetCustomerLookupHoldsAsync(id),
                CustomerLookupHistoryCount = await _customerLookupService.GetCustomerLookupHistoryCountAsync(id),
                Search = search
            });
        }

        public async Task<IActionResult> GetCustomerLookupHistory(int customerLookupID, string search, int orderBy,
            bool orderDesc, int page = 1)
        {
            search = search?.Trim();
            var filter = new MaterialFilter(page)
            {
                CustomerLookupID = customerLookupID,
                Search = search,
                OrderBy = (MaterialFilter.OrderType)orderBy,
                OrderDesc = orderDesc
            };

            var itemList = await _customerLookupService.GetPaginatedCustomerLookupHistoryAsync(filter);

            var paginateModel = new PaginateModel
            {
                ItemCount = itemList.Count,
                CurrentPage = page,
                ItemsPerPage = filter.Take.Value
            };

            return PartialView("_HistoryPartial", new HistoryViewModel
            {
                Items = itemList.Data,
                PaginateModel = paginateModel,
                OrderBy = orderBy,
                OrderDesc = orderDesc
            });
        }

        [HttpPost("[action]")]
        public async Task<JsonResult> UpdateCustomerField(int id, string field, string text)
        {
            string message = string.Empty;
            bool success = false;

            var customer = await _booksByMailService.FindAsync(id);

            if (customer == null)
            {
                _logger.LogError("Could not find customer {CustomerId} for updating {FieldName}", id, field);
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
                    _booksByMailService.Update(customer);
                    success = true;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error updating field {field} for customer {id}. {ex}", id, field, ex);
                    message = $"Error updating customer {field}.";
                }
            }

            return Json(new { success, message, text });
        }

        [HttpPost("[action]")]
        public async Task<JsonResult> AddComment(int id, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return Json(new { success = false, message = "Comment text cannot be empty." });
            }

            string message = string.Empty;
            bool success = false;

            var customer = await _booksByMailService.FindAsync(id);

            if (customer == null)
            {
                _logger.LogError("Could not find customer {id} for adding comment", id);
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
                comment = await _booksByMailService.AddCommentAsync(comment);
                success = true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error adding comment for customer {id}. {ex}", id, ex);
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