using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorPagesRecipes.Protos;
using System.Text.Json;

namespace RazorPagesRecipes.Pages.Recipes
{
    public class RecipeDetailsModel : PageModel
    {
        private readonly RazorPagesRecipes.Protos.Recipes.RecipesClient _recipesClient;
        public Protos.RecipeModel RecipeDetails { get; set; } = new();
        public RecipeDetailsModel(RazorPagesRecipes.Protos.Recipes.RecipesClient recipeClient)
        {
            _recipesClient = recipeClient;
        }
        public async Task OnGet(Guid id)
        {
            try
            {
                RecipeDetails=_recipesClient.GetRecipe(new Protos.RecipeLookUpModel { Id = id.ToString() });
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
    }
}
