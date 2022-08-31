using Grpc.Core;
using GrpcRecipeApp;
using GrpcRecipeApp.Protos;
using System.Text.Json;

namespace GrpcRecipeApp.Services
{
    public class CategoriesService : Categories.CategoriesBase
    {
        private readonly ILogger<CategoriesService> _logger;
        private List<string> _categories = new();
        private Protos.RecipeListModel _recipes = new();
        public CategoriesService(ILogger<CategoriesService> logger)
        {
            _logger = logger;
        }

        public override async Task GetCategories(Void request, IServerStreamWriter<CategoryModel> responseStream, ServerCallContext context)
        {

            await LoadCaregories();
            Console.WriteLine(_categories);
            foreach (var categoryItem in _categories)
            {
                await responseStream.WriteAsync(new CategoryModel { Category = categoryItem });
            }
        }

        public override async Task<CategoryModel> CreateCategory(CategoryModel request, ServerCallContext context)
        {
            await LoadCaregories();
            string newCategory = request.Category;
            if (_categories.Contains(newCategory))
            {
                throw new RpcException(new Status(StatusCode.PermissionDenied, "Entered category already exist"));

            }
            else if(newCategory == "")
            {
                throw new RpcException(new Status(StatusCode.PermissionDenied, "The input is empty"));
            }
            else
            {
                _categories.Add(newCategory);
                await SaveCategoryToJson();
                return request;
            }
        }

        public override async Task<CategoryModel> UpdateCategory(UpdateCategoryRequest request, ServerCallContext context)
        {
            await LoadCaregories();
            string newCategory = request.NewCategoryName;
            string oldCategory = request.OldCategoryName;
            int indexOfCategory = _categories.FindIndex(x => x == oldCategory);
            if (indexOfCategory != -1 && !_categories.Contains(newCategory) && newCategory != "")
            {
                _categories[indexOfCategory] = newCategory;

                //Edit the category name for each recipe belog to this category
                for (int i = 0; i < _recipes.RecipesList.Count; i++)
                {
                    for (int j = 0; j < _recipes.RecipesList[i].Categories.Count; j++)
                    {
                        if (_recipes.RecipesList[i].Categories[j] == oldCategory)
                        {
                            _recipes.RecipesList[i].Categories[j] = newCategory;
                        }
                    }
                }
                await SaveCategoryToJson();
                await SaveRecipeToJson();
                return new CategoryModel { Category = newCategory };
            }
            else
            {
                throw new RpcException(new Status(StatusCode.PermissionDenied, "Permission denied"));
            }
        }

        public override async Task<CategoryModel> DeleteCategory(CategoryModel request, ServerCallContext context)
        {
            await LoadRecipes();
            await LoadCaregories();
            string deleteCategory = request.Category;
            if (_categories.Contains(deleteCategory))
            {
                _categories.Remove(deleteCategory);

                //Remove this category from each recipe
                for (int i = 0; i < _recipes.RecipesList.Count; i++)
                {
                    for (int j = 0; j < _recipes.RecipesList[i].Categories.Count; j++)
                    {
                        if (_recipes.RecipesList[i].Categories[j] == deleteCategory)
                        {
                            _recipes.RecipesList[i].Categories.Remove(_recipes.RecipesList[i].Categories[j]);
                        }
                    }
                }
                await SaveCategoryToJson();
                await SaveRecipeToJson();
                return new CategoryModel { Category = deleteCategory };
            }
            else
            {
                throw new RpcException(new Status(StatusCode.PermissionDenied, "Permission denied"));
            }
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

        async Task SaveRecipeToJson()
        {
            string jsonString = JsonSerializer.Serialize(_recipes);
            await WriteJsonFile("recipe", jsonString);
        }

        public async Task SaveCategoryToJson()
        {
            string jsonString = JsonSerializer.Serialize(_categories);
            await WriteJsonFile("category", jsonString);
        }
    }
}
