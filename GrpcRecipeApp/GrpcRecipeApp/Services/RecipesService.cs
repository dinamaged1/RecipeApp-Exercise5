using System;
using Grpc.Core;
using GrpcRecipeApp;
using GrpcRecipeApp.Protos;
using System.Text.Json;

namespace GrpcRecipeApp.Services
{
    public class RecipesService : Recipes.RecipesBase
    {
        private readonly ILogger<CategoriesService> _logger;
        private List<string> _categories = new();
        private RecipeListModel _recipes = new();
        public RecipesService(ILogger<CategoriesService> logger)
        {
            _logger = logger;
        }

        public RecipesService()
        {
        }

        public override async Task GetRecipes(Protos.Void request, IServerStreamWriter<RecipeModel> responseStream, ServerCallContext context)
        {
            await LoadRecipes();
            foreach (RecipeModel recipe in _recipes.RecipesList)
            {
                await responseStream.WriteAsync(recipe);
            }
        }

        public override Task<RecipeModel> CreateRecipe(RecipeModel request, ServerCallContext context)
        {
            return base.CreateRecipe(request, context);
        }

        public async Task TestAddRecipe(){
            RecipeModel newRecipe = new ();
            newRecipe.Id = "f3eb3803-76af-4e0e-805a-956344dd7eb1";
            newRecipe.Title = "Hot Cocoa";
            newRecipe.Ingredients.Add("ing1");
            newRecipe.Instructions.Add("ins1");
            newRecipe.Instructions.Add("ins2");
            newRecipe.Categories.Add("cat1");
            _recipes.RecipesList.Add(newRecipe);
            await SaveRecipeToJson();
    }
        public async Task LoadCaregories()
        {
            string categoryJson = await ReadJsonFile("category");
            if (categoryJson == null) { return; }
            _categories = JsonSerializer.Deserialize<List<string>>(categoryJson)!;
        }

        public async Task LoadRecipes()
        {
            string recipeJson = await ReadJsonFile("recipe");
            if (recipeJson == null) { return; }
            _recipes = JsonSerializer.Deserialize<RecipeListModel>(recipeJson)!;
        }

        public async Task<string> ReadJsonFile(string fileName) =>
            await File.ReadAllTextAsync($"{fileName}.json");

        public async Task WriteJsonFile(string fileName, string fileData) =>
            await File.WriteAllTextAsync($"{fileName}.json", fileData);

        public async Task SaveRecipeToJson()
        {
            string jsonString = JsonSerializer.Serialize(_recipes);
            await WriteJsonFile("recipe", jsonString);
        }

    }
}
