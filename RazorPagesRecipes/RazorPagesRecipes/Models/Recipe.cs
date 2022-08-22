using System.Text.Json.Serialization;

namespace RazorPagesRecipes.Models
{
    public class Recipe
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; } = String.Empty;
        [JsonPropertyName("imagepath")]
        public string Imagepath { get; set; } = String.Empty;
        [JsonPropertyName("ingredients")]
        public List<string> Ingredients { get; set; } = new();
        [JsonPropertyName("instructions")]
        public List<string> Instructions { get; set; } = new();
        [JsonPropertyName("categories")]

        public List<string> Categories { get; set; } = new();
        public Recipe(Guid id, string title, string imagePath, List<string> instructions, List<string> ingredients, List<string> categories)
        {
            Id = id;
            Title = title;
            Instructions = instructions;
            Ingredients = ingredients;
            Categories = categories;
            Imagepath = imagePath;
        }
        public Recipe()
        {

        }
    }
}
