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
        private List<RecipeModel> _recipes = new();
        public RecipesService(ILogger<CategoriesService> logger)
        {
            _logger = logger;
        }

        public override async Task GetRecipes(Protos.Void request, IServerStreamWriter<RecipeModel> responseStream, ServerCallContext context)
        {
            await LoadRecipes();
            foreach (RecipeModel recipe in _recipes)
            {
                await responseStream.WriteAsync(recipe);
            }
        }

        public override async Task<RecipeModel> GetRecipe(RecipeLookUpModel request, ServerCallContext context)
        {
            var selectedRecipe = _recipes.FirstOrDefault(x => x.Id == request.Id);
            if (selectedRecipe == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Recipe not found"));
            }
            else
            {
                return selectedRecipe;
            }
        }

        public override async Task<RecipeModel> CreateRecipe(RecipeModel request, ServerCallContext context)
        {
            if(request == null )
            {
                throw new RpcException(new Status(StatusCode.PermissionDenied, "Permission denied"));
            }
            if(request.Title == string.Empty || request.Id == string.Empty || request.ImagePath == string.Empty || request.Ingredients == null || request.Instructions == null || request.Categories == null)
            {
                throw new RpcException(new Status(StatusCode.PermissionDenied, "Inputs are not complete"));
            }
            else
            {
                _recipes.Add(request);
                await SaveRecipeToJson();
                return request;
            }
        }

        public override async Task<RecipeModel> UpdateRecipe(UpdateRecipeRequest request, ServerCallContext context)
        {
            if (request == null)
            {
                throw new RpcException(new Status(StatusCode.PermissionDenied, "Permission denied"));
            }
            else if (request.EditedRecipe.Title == string.Empty || request.EditedRecipe.Id == string.Empty || request.EditedRecipe.ImagePath == string.Empty || request.EditedRecipe.Ingredients == null || request.EditedRecipe.Instructions == null || request.EditedRecipe.Categories == null)
            {
                throw new RpcException(new Status(StatusCode.PermissionDenied, "Inputs are not complete"));
            }
            else
            {
                var selectedRecipe= _recipes.FirstOrDefault(x => x.Id == request.Id);
                var selectedRecipeIndex= _recipes.IndexOf(selectedRecipe);
                if (selectedRecipeIndex == -1)
                {
                    throw new RpcException(new Status(StatusCode.PermissionDenied, "Permission denied"));
                }
                else
                {
                    _recipes[selectedRecipeIndex] = request.EditedRecipe;
                    await SaveRecipeToJson();
                    return request.EditedRecipe;
                }
            }
        }

        public override async Task<RecipeModel> DeleteRecipe(RecipeLookUpModel request, ServerCallContext context)
        {
            var selectedRecipe=_recipes.FirstOrDefault(x => x.Id == request.Id);
            if (selectedRecipe == null)
            {
                throw new RpcException(new Status(StatusCode.PermissionDenied, "Permission denied"));
            }
            else
            {
                _recipes.Remove(selectedRecipe);
                await SaveRecipeToJson();
                return selectedRecipe;
            }
        }

        public async Task TestAddRecipe(){
            RecipeModel newRecipe = new ();
            newRecipe.Id = "f3eb3803-76af-4e0e-805a-956344dd7eb1";
            newRecipe.Title = "Hot Cocoa";
            newRecipe.Ingredients.Add("ing1");
            newRecipe.Instructions.Add("ins1");
            newRecipe.Instructions.Add("ins2");
            newRecipe.Categories.Add("cat1");
            _recipes.Add(newRecipe);
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
            _recipes = JsonSerializer.Deserialize<List<RecipeModel>>(recipeJson)!;
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
