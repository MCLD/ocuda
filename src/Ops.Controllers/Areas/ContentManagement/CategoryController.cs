using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Category;
using Ocuda.Ops.Controllers.ServiceFacades;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Utility.Abstract;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement
{
    [Area(nameof(ContentManagement))]
    [Route("[area]/[controller]")]
    public class CategoryController(
        Controller<CategoryController> context,
        IDateTimeProvider dateTimeProvider,
        ICategoryRepository categoryRepository)
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

        [HttpGet("")]
        [HttpGet("{page:int}")]
        public async Task<IActionResult> Index(int? page)
        {
            var filter = new BaseFilter(page);

            var viewModel = new IndexViewModel
            {
                Categories = await categoryRepository.PageAsync(filter),
                Page = filter.Page,
            };

            return View(viewModel);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Add(IndexViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                await categoryRepository.AddAsync(new Models.Entities.Category
                {
                    CreatedAt = dateTimeProvider.Now,
                    CreatedBy = CurrentUserId,
                    Name = viewModel.CategoryName,
                    Slug = viewModel.CategorySlug,
                });

                await categoryRepository.SaveAsync();
            }
            else
            {
                var issues = new StringBuilder();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        issues.Append("<li>").Append(error.ErrorMessage).Append("</li>");
                    }
                }

                ShowAlertDanger($"Unable to add section category:{issues}");
            }

            return RedirectToAction(nameof(Index), new { page = viewModel.Page });
        }
    }
}
