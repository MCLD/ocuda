using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Category;
using Ocuda.Ops.Controllers.ServiceFacades;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Exceptions;
using Org.BouncyCastle.Utilities.Encoders;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement
{
    [Area(nameof(ContentManagement))]
    [Route("[area]/[controller]")]
    public class CategoryController(
        Controller<CategoryController> context,
        IDateTimeProvider dateTimeProvider,
        IContentCategoryService contentCategoryService)
        : BaseController<CategoryController>(context)
    {
        public static string Area
        {
            get { return nameof(ContentManagement); }
        }

        public static string Name
        {
            get { return "Category"; }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Add(IndexViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await contentCategoryService.AddAsync(new Models.Entities.Category
                    {
                        CreatedAt = dateTimeProvider.Now,
                        CreatedBy = CurrentUserId,
                        Name = viewModel.CategoryName,
                        Slug = viewModel.CategorySlug,
                    });
                }
                catch (OcudaException oex)
                {
                    ShowAlertDanger(
                        $"Unable to add category: {HttpUtility.HtmlEncode(oex.Message)}");
                }
            }
            else
            {
                var issues = new StringBuilder();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        issues.Append("<li>")
                            .Append(HttpUtility.HtmlEncode(error.ErrorMessage))
                            .Append("</li>");
                    }
                }

                ShowAlertDanger($"Unable to add section category:{issues}");
            }

            return RedirectToAction(nameof(Index), new { page = viewModel.Page });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Delete(int deleteCategoryId, int? page)
        {
            try
            {
                _logger.LogInformation(
                    "Attempting to delete category ID: {CategoryId}",
                    deleteCategoryId);
                await contentCategoryService.RemoveAsync(deleteCategoryId);
            }
            catch (ArgumentNullException anex)
            {
                context.Logger.LogError(anex,
                    "Asked to delete nonexistent category id {CategoryId}",
                    deleteCategoryId);
                ShowAlertDanger($"Unable to find category id {deleteCategoryId}");
            }
            catch (OcudaException oex)
            {
                context.Logger.LogError("Unable to delete category id {CategoryId}: {ErrorMessage}",
                    deleteCategoryId,
                    oex.Message);
                ShowAlertDanger($"Unable to delete category id {deleteCategoryId}: {HttpUtility.HtmlEncode(oex.Message)}");
            }

            return RedirectToAction(nameof(Index), new { page });
        }

        [HttpGet("")]
        [HttpGet("{page:int}")]
        public async Task<IActionResult> Index(int? page)
        {
            var filter = new BaseFilter(page != 0 ? page : 1);

            var categories = await contentCategoryService.PageAsync(filter);

            var mapping = categories.Data
                .ToDictionary(k => k.Id, v => v.UsedInSections.Select(_ => new { _.Name, _.Slug }));

            var link = Url.Action(
                nameof(SectionController.Categories),
                SectionController.Name,
                new { slug = '0' }).TrimEnd('0');

            var viewModel = new IndexViewModel
            {
                Categories = categories.Data,
                CategorySectionLink = link,
                CategorySectionMappingJson = JsonSerializer.Serialize(mapping),
                CurrentPage = filter.Page,
                HasManagementPermission = IsSiteManager(),
                ItemCount = categories.Count,
                ItemsPerPage = filter.Take.Value,
            };

            if (viewModel.PastMaxPage)
            {
                return RedirectToRoute(new { page = viewModel.LastPage ?? 1 });
            }

            if (categories.Count == 0)
            {
                ShowAlertWarning("No categories configured.");
            }

            return View(viewModel);
        }
    }
}
