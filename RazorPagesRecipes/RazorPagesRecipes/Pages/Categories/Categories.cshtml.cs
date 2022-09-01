using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Grpc.Core;

namespace RazorPagesRecipes.Pages.Categories
{
    public class CategoriesModel : PageModel
    {
        private readonly RazorPagesRecipes.Categories.CategoriesClient _client;
        public bool IsRequestSucceed { get; }
        public List<string> Categories { get; set; } = new List<string>();
        [BindProperty]
        public string CategoryNew { get; set; } = String.Empty;
        [BindProperty]
        public string CategoryOld { get; set; } = String.Empty;
        [BindProperty]
        public string ToBeDeletedCategory { get; set; } = String.Empty;
        [BindProperty]
        public string Message { get; set; } = String.Empty;

        public CategoriesModel(RazorPagesRecipes.Categories.CategoriesClient categoriesClient)
        {
            _client = categoriesClient;
        }

        // Get all categories
        public async Task OnGet()
        {
            var getCategoriesReply = _client.GetCategories(new RazorPagesRecipes.Void());

            using var tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;

            await foreach (var cate in getCategoriesReply.ResponseStream.ReadAllAsync(token))
            {
                Categories.Add(cate.Category);
            }
        }

        // Add new category
        public async Task<IActionResult> OnPostAdd()
        {
            try
            {
                var createCategoriesReply = await _client.CreateCategoryAsync(new CategoryModel { Category = CategoryNew });
            }
            catch (Exception ex)
            {
                TempData["confirmation"] = "failed";
                TempData["details"] = $"{CategoryNew} category already exist!";
                return RedirectToPage("Categories");
            }
            TempData["confirmation"] = "succeed";
            TempData["details"] = $"{CategoryNew} category added successfully 😁";
            return RedirectToPage("Categories");

        }

        // Edit a category
        public async Task<IActionResult> OnPostUpdate()
        {
            if (CategoryNew != CategoryOld)
            {
                try
                {
                    var createCategoriesReply = _client.UpdateCategory(new UpdateCategoryRequest { OldCategoryName = CategoryOld, NewCategoryName = CategoryNew });
                }
                catch (Exception ex)
                {
                    TempData["confirmation"] = "failed";
                    TempData["details"] = $"Error occurred while editing {CategoryOld} category! Please try again later";
                    return RedirectToPage("Categories");
                }
                TempData["confirmation"] = "succeed";
                TempData["details"] = $"{CategoryOld} category edited successfully 😁";
                return RedirectToPage("Categories");
            }
            else
            {
                return RedirectToPage("Categories");
            }
        }

        public async Task<IActionResult> OnPostDelete()
        {
            try
            {
                _client.DeleteCategory(new CategoryModel { Category = ToBeDeletedCategory });
            }
            catch (Exception ex)
            {
                TempData["confirmation"] = "failed";
                TempData["details"] = $"Error occurred while deleting {ToBeDeletedCategory} category. Please try again later";
                return RedirectToPage("Categories");
            }
            TempData["confirmation"] = "succeed";
            TempData["details"] = $"{ToBeDeletedCategory} category deleted successfully";
            return RedirectToPage("Categories");
        }
    }
}
