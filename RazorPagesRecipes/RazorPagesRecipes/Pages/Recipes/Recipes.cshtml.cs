using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;
using RazorPagesRecipes.Models;

namespace RazorPagesRecipes.Pages.Recipes
{
    public class RecipesModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _httpClient;
        private IWebHostEnvironment _host;
        [BindProperty]
        public List<string> Categories { get; set; } = new List<string>();
        public List<Recipe> Recipes { get; set; } = new List<Recipe>();
        [BindProperty]
        public IFormFile? RecipeImage { get; set; }
        [BindProperty]
        public List<bool> IsCheckedCategory { get; set; } = new List<bool>();
        [BindProperty]
        public Guid ChangebleId { get; set; }
        [BindProperty]
        public string ChangebleImagePath { get; set; }

        public RecipesModel(IHttpClientFactory client, IWebHostEnvironment host)
        {
            _httpClientFactory = client;
            _httpClient = _httpClientFactory.CreateClient("recipe");
            _host = host;
        }
        public async Task OnGet()
        {
            var httpResponseMessage =
                await _httpClient.GetAsync($"/categories");
            bool isRequestSucceed = httpResponseMessage.IsSuccessStatusCode;
            var categoryData = await httpResponseMessage.Content.ReadAsStringAsync();

            httpResponseMessage =
                await _httpClient.GetAsync($"/recipes");
            httpResponseMessage.EnsureSuccessStatusCode();
            var recipeData = httpResponseMessage.Content.ReadAsStringAsync().Result;

            Categories = JsonSerializer.Deserialize<List<string>>(categoryData);
            Recipes = JsonSerializer.Deserialize<List<Recipe>>(recipeData);

            IsCheckedCategory = new List<bool>();
            IsCheckedCategory = new List<bool>(Categories.Count);
            Categories.ForEach(x => IsCheckedCategory.Add(false));
        }

        public async Task<IActionResult> OnPostAdd()
        {
            var httpResponseMessage =
                await _httpClient.GetAsync($"/categories");
            bool isRequestSucceed = httpResponseMessage.IsSuccessStatusCode;
            var categoryData = await httpResponseMessage.Content.ReadAsStringAsync();
            Categories = JsonSerializer.Deserialize<List<string>>(categoryData);
            Guid id = Guid.NewGuid();

            // Get the image uploaded 
            // Save it to RecipeImages folder
            MemoryStream ms = new MemoryStream();
            await RecipeImage.OpenReadStream().CopyToAsync(ms);
            var data = ms.ToArray();
            var filePath = $"{_host.WebRootPath}/RecipesImages/{id}.{System.IO.Path.GetExtension(RecipeImage.FileName)}";
            System.IO.File.WriteAllBytes(filePath, data);

            string recipeTitle = Request.Form["title"];
            string recipeInstructionsInput = Request.Form["instructions"];
            string recipeIngredientsInput = Request.Form["ingredients"];
            List<string> ingredientsList = recipeInstructionsInput.Split('\n').ToList();
            List<string> instructionsList = recipeIngredientsInput.Split('\n').ToList();
            string imagePath = $"/RecipesImages/{id}.{System.IO.Path.GetExtension(RecipeImage.FileName)}";
            List<string> categoryList = new List<string>();
            for (int i = 0; i < IsCheckedCategory.Count; i++)
            {
                if (IsCheckedCategory[i] == true)
                {
                    categoryList.Add(Categories[i]);
                }
            }
            Recipe newRecipe = new Recipe(id, recipeTitle, imagePath, ingredientsList, instructionsList, categoryList);
            var recipeItemJson = new StringContent(JsonSerializer.Serialize(newRecipe), Encoding.UTF8, "application/json");
            httpResponseMessage = await _httpClient.PostAsync("/recipe", recipeItemJson);
            try { httpResponseMessage.EnsureSuccessStatusCode(); }
            catch (Exception ex)
            {
                TempData["confirmation"] = "failed";
                TempData["details"] = $"{recipeTitle} recipe already exist!";
                return RedirectToPage("Recipes");
            }
            TempData["confirmation"] = "succeed";
            TempData["details"] = $"{recipeTitle} recipe added successfully 😁";
            return RedirectToPage("Recipes");
        }

        public async Task<IActionResult> OnPostUpdate()
        {
            var httpResponseMessage =
                await _httpClient.GetAsync($"/categories");
            bool isRequestSucceed = httpResponseMessage.IsSuccessStatusCode;
            var categoryData = await httpResponseMessage.Content.ReadAsStringAsync();
            Categories = JsonSerializer.Deserialize<List<string>>(categoryData);

            //Delete Image from folder RecipeImages
            var filePathDelete = $"{_host.WebRootPath}{ChangebleImagePath}";
            System.IO.File.Delete(filePathDelete);

            Guid idNew = Guid.NewGuid();
            // Get the image uploaded 
            // Save it to RecipeImages folder
            MemoryStream ms = new MemoryStream();
            await RecipeImage.OpenReadStream().CopyToAsync(ms);
            var data = ms.ToArray();
            var filePath = $"{_host.WebRootPath}/RecipesImages/{idNew}.{System.IO.Path.GetExtension(RecipeImage.FileName)}";
            System.IO.File.WriteAllBytes(filePath, data);

            string recipeTitle = Request.Form["title"];
            string recipeInstructionsInput = Request.Form["instructions"];
            string recipeIngredientsInput = Request.Form["ingredients"];
            List<string> ingredientsList = recipeInstructionsInput.Split('\n').ToList();
            List<string> instructionsList = recipeIngredientsInput.Split('\n').ToList();
            string imagePath = $"/RecipesImages/{idNew}.{System.IO.Path.GetExtension(RecipeImage.FileName)}";
            List<string> categoryList = new List<string>();
            for (int i = 0; i < IsCheckedCategory.Count; i++)
            {
                if (IsCheckedCategory[i] == true)
                {
                    categoryList.Add(Categories[i]);
                }
            }
            Recipe newRecipe = new Recipe(idNew, recipeTitle, imagePath, ingredientsList, instructionsList, categoryList);
            var recipeItemJson = new StringContent(JsonSerializer.Serialize(newRecipe), Encoding.UTF8, "application/json");

            httpResponseMessage =
                await _httpClient.PutAsync($"/recipe/{ChangebleId}", recipeItemJson);

            try { httpResponseMessage.EnsureSuccessStatusCode(); }
            catch (Exception ex)
            {
                TempData["confirmation"] = "failed";
                TempData["details"] = $"Error occurred while editing! Please try again later";
                return RedirectToPage("Recipes");
            }
            TempData["confirmation"] = "succeed";
            TempData["details"] = $"Recipe edited successfully 😁";
            return RedirectToPage("Recipes");
        }

        public async Task<IActionResult> OnPostDelete()
        {
            //Delete Image from folder RecipeImages
            var filePath = $"{_host.WebRootPath}{ChangebleImagePath}";
            System.IO.File.Delete(filePath);

            using var httpResponseMessage =
                await _httpClient.DeleteAsync($"/recipe/{ChangebleId}");

            try { httpResponseMessage.EnsureSuccessStatusCode(); }
            catch (Exception ex)
            {
                TempData["confirmation"] = "failed";
                TempData["details"] = $"Error occurred while deleting recipe. Please try again later";
                return RedirectToPage("Recipes");
            }
            TempData["confirmation"] = "succeed";
            TempData["details"] = $"Recipe deleted successfully";
            return RedirectToPage("Recipes");
        }
    }
}
