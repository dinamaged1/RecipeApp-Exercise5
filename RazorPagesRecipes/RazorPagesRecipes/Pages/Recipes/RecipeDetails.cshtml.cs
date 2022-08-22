using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorPagesRecipes.Models;
using System.Text.Json;

namespace RazorPagesRecipes.Pages.Recipes
{
    public class RecipeDetailsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _httpClient;
        public Recipe? RecipeDetails { get; set; }
        public RecipeDetailsModel(IHttpClientFactory client)
        {
            _httpClientFactory = client;
            _httpClient = _httpClientFactory.CreateClient("recipe");
        }
        public async Task OnGet(Guid id)
        {
            var httpResponseMessage =
                await _httpClient.GetAsync($"/recipe/{id}");
            var recipeData = httpResponseMessage.Content.ReadAsStringAsync().Result;
            RecipeDetails = JsonSerializer.Deserialize<Recipe>(recipeData);

        }
    }
}
