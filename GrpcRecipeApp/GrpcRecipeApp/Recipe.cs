﻿namespace GrpcRecipeApp
{
    public class Recipe
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = String.Empty;
        public List<string> Instructions { get; set; } = new();
        public List<string> Ingredients { get; set; } = new();
        public List<string> Categories { get; set; } = new();
    }
}
