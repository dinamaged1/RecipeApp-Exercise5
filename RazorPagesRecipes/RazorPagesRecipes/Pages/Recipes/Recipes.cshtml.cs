using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;
using Grpc.Core;

namespace RazorPagesRecipes.Pages.Recipes
{
    public class RecipesModel : PageModel
    {
        private readonly RazorPagesRecipes.Protos.Recipes.RecipesClient _recipesClient;
        RazorPagesRecipes.Categories.CategoriesClient _categoriesClient;
        private IWebHostEnvironment _host;
        [BindProperty]
        public List<string> Categories { get; set; } = new List<string>();
        public List<RazorPagesRecipes.Protos.RecipeModel> Recipes { get; set; } = new List<RazorPagesRecipes.Protos.RecipeModel>();
        [BindProperty]
        public IFormFile? RecipeImage { get; set; }
        [BindProperty]
        public List<bool> IsCheckedCategory { get; set; } = new List<bool>();
        [BindProperty]
        public Guid ChangebleId { get; set; }
        [BindProperty]
        public string ChangebleImagePath { get; set; } = string.Empty;

        public RecipesModel(RazorPagesRecipes.Protos.Recipes.RecipesClient recipeClient, RazorPagesRecipes.Categories.CategoriesClient categoriesClient, IWebHostEnvironment host)
        {
            _recipesClient = recipeClient;
            _categoriesClient = categoriesClient;
            _host = host;
        }
        public async Task OnGet()
        {
            var getCategoriesReply = _categoriesClient.GetCategories(new RazorPagesRecipes.Void());

            using var tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;

            await foreach (var cate in getCategoriesReply.ResponseStream.ReadAllAsync(token))
            {
                Categories.Add(cate.Category);
            }

            var getRecipesReply = _recipesClient.GetRecipes(new RazorPagesRecipes.Protos.Void());

            await foreach (var recipe in getRecipesReply.ResponseStream.ReadAllAsync())
            {
                Recipes.Add(recipe);
            }

            IsCheckedCategory = new List<bool>();
            IsCheckedCategory = new List<bool>(Categories.Count);
            Categories.ForEach(x => IsCheckedCategory.Add(false));
        }

        public async Task<IActionResult> OnPostAdd()
        {
            var getCategoriesReply = _categoriesClient.GetCategories(new RazorPagesRecipes.Void());

            using var tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;

            await foreach (var cate in getCategoriesReply.ResponseStream.ReadAllAsync(token))
            {
                Categories.Add(cate.Category);
            }

            Guid id = Guid.NewGuid();
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

            RazorPagesRecipes.Protos.RecipeModel newRecipe = new RazorPagesRecipes.Protos.RecipeModel { Id = id.ToString(), Title = recipeTitle, ImagePath = imagePath };
            foreach (var ingredient in ingredientsList)
            {
                newRecipe.Ingredients.Add(ingredient);
            }
            foreach (var instruction in instructionsList)
            {
                newRecipe.Instructions.Add(instruction);
            }
            foreach (var category in categoryList)
            {
                newRecipe.Categories.Add(category);
            }
            try { 
                var createRecipeReply =await _recipesClient.CreateRecipeAsync(newRecipe);

                // Get the image uploaded 
                // Save it to RecipeImages folder
                MemoryStream ms = new MemoryStream();
                await RecipeImage.OpenReadStream().CopyToAsync(ms);
                var data = ms.ToArray();
                var filePath = $"{_host.WebRootPath}/RecipesImages/{id}.{System.IO.Path.GetExtension(RecipeImage.FileName)}";
                System.IO.File.WriteAllBytes(filePath, data);
            }
            catch (Exception ex)
            {
                TempData["confirmation"] = "failed";
                TempData["details"] = $"{recipeTitle} recipe not added!";
                return RedirectToPage("Recipes");
            }
            TempData["confirmation"] = "succeed";
            TempData["details"] = $"{recipeTitle} recipe added successfully 😁";
            return RedirectToPage("Recipes");
        }

        public async Task<IActionResult> OnPostUpdate()
        {
            var getCategoriesReply = _categoriesClient.GetCategories(new RazorPagesRecipes.Void());

            using var tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;

            await foreach (var cate in getCategoriesReply.ResponseStream.ReadAllAsync(token))
            {
                Categories.Add(cate.Category);
            }

            Guid idNew = Guid.NewGuid();
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
            RazorPagesRecipes.Protos.RecipeModel newRecipe = new RazorPagesRecipes.Protos.RecipeModel { Id = idNew.ToString(), Title = recipeTitle, ImagePath = imagePath };
            foreach (var ingredient in ingredientsList)
            {
                newRecipe.Ingredients.Add(ingredient);
            }
            foreach (var instruction in instructionsList)
            {
                newRecipe.Instructions.Add(instruction);
            }
            foreach (var category in categoryList)
            {
                newRecipe.Categories.Add(category);
            }

            try {
                var updateRecipeReply = _recipesClient.UpdateRecipe(new Protos.UpdateRecipeRequest { Id=ChangebleId.ToString(),EditedRecipe=newRecipe});

                //Delete Image from folder RecipeImages
                var filePathDelete = $"{_host.WebRootPath}{ChangebleImagePath}";
                System.IO.File.Delete(filePathDelete);

                // Get the image uploaded 
                // Save it to RecipeImages folder
                MemoryStream ms = new MemoryStream();
                await RecipeImage.OpenReadStream().CopyToAsync(ms);
                var data = ms.ToArray();
                var filePath = $"{_host.WebRootPath}/RecipesImages/{idNew}.{System.IO.Path.GetExtension(RecipeImage.FileName)}";
                System.IO.File.WriteAllBytes(filePath, data);

            }
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
            try
            {
                var deleteRecipeReply =await _recipesClient.DeleteRecipeAsync(new Protos.RecipeLookUpModel { Id = ChangebleId.ToString() });
            }
            catch (Exception ex)
            {
                TempData["confirmation"] = "failed";
                TempData["details"] = $"Error occurred while deleting recipe. Please try again later";
                return RedirectToPage("Recipes");
            }

            //Delete Image from folder RecipeImages
            var filePath = $"{_host.WebRootPath}{ChangebleImagePath}";
            System.IO.File.Delete(filePath);

            TempData["confirmation"] = "succeed";
            TempData["details"] = $"Recipe deleted successfully";
            return RedirectToPage("Recipes");
        }
    }
}
