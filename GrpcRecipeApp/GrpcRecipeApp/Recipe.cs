namespace GrpcRecipeApp
{
    public class Recipe
    {
        public string Id { get; set; } = String.Empty;
        public string Title { get; set; } = String.Empty;
        public string Image = String.Empty;
        public List<string> Ingredients { get; set; } = new();
        public List<string> Instructions { get; set; } = new();
        public List<string> Categories { get; set; } = new();
    }
}
