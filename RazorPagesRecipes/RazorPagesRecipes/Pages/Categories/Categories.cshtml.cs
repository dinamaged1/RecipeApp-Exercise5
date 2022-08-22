using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorPagesRecipes;
using System.Text;
using System.Text.Json;

namespace RazorPagesRecipes.Pages.Categories
{
    public class CategoriesModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _httpClient;
        public bool IsRequestSucceed { get; }
        public List<string> Categories { get; set; } = new List<string>();
        [BindProperty]
        public string CategoryNew { get; set; }
        [BindProperty]
        public string CategoryOld { get; set; }
        [BindProperty]
        public string ToBeDeletedCategory { get; set; }
        [BindProperty]
        public string Message { get; set; }

        public CategoriesModel(IHttpClientFactory client)
        {
            _httpClientFactory = client;
            _httpClient = _httpClientFactory.CreateClient("recipe");
        }


        // Get all categories
        public async Task OnGet()
        {
            var httpResponseMessage =
                 await _httpClient.GetAsync($"/categories");
            bool isRequestSucceed = httpResponseMessage.IsSuccessStatusCode;
            var categoryData = await httpResponseMessage.Content.ReadAsStringAsync();
            Categories = JsonSerializer.Deserialize<List<string>>(categoryData);
        }

        // Add new category
        public async Task<IActionResult> OnPostAdd()
        {
            var categoryItemJson = new StringContent(
                        JsonSerializer.Serialize(CategoryNew),
                        Encoding.UTF8,
                        "application/json");

            using var httpResponseMessage =
                await _httpClient.PostAsync("/category", categoryItemJson);
            try { httpResponseMessage.EnsureSuccessStatusCode(); }
            catch (Exception ex)
            {
                TempData["confirmation"] = "failed";
                TempData["details"] = $"{CategoryNew} category already exist!";
                return RedirectToPage("Categories");
            }
            TempData["confirmation"] = "succeed";
            TempData["details"]= $"{CategoryNew} category added successfully 😁";
            return RedirectToPage("Categories");

        }

        // Edit a category
        public async Task<IActionResult> OnPostUpdate()
        {
            if (CategoryNew != CategoryOld)
            {
                var newCategoryJson = new StringContent(
                    JsonSerializer.Serialize(CategoryNew),
                    Encoding.UTF8,
                    "application/json");

                using var httpResponseMessage =
                    await _httpClient.PutAsync($"/category/{CategoryOld}", newCategoryJson);
                try { httpResponseMessage.EnsureSuccessStatusCode(); }
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
            using var httpResponseMessage =
                await _httpClient.DeleteAsync($"/category/{ToBeDeletedCategory}");

            try { httpResponseMessage.EnsureSuccessStatusCode(); }
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
