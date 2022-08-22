using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorPagesRecipes;
using static System.Net.Mime.MediaTypeNames;

namespace RazorPagesRecipes.Pages.Recipes
{
    public class CreateRecipeModel : PageModel
    {
        [BindProperty]
        public IFormFile? RecipeImage { get; set; }
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _httpClient;
        private IWebHostEnvironment _host;

        public CreateRecipeModel(IHttpClientFactory client, IWebHostEnvironment host)
        {
            _httpClientFactory = client;
            _httpClient = _httpClientFactory.CreateClient("recipe");
            _host = host;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAdd()
        {

            var x = RecipeImage;
            MemoryStream ms = new MemoryStream();

            RecipeImage.OpenReadStream().CopyToAsync(ms);
            var data = ms.ToArray();
            Guid id = Guid.NewGuid();
            
            var filePath=$"{_host.WebRootPath}/RecipesImages/{id}.{System.IO.Path.GetExtension(RecipeImage.FileName)}";
            System.IO.File.WriteAllBytes(filePath, data);
            return Page();

        }

    }
}
