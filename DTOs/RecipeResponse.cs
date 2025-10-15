namespace YemekTarifAPI.DTOs
{
    public class RecipeResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Ingredients { get; set; }
        public string Steps { get; set; }
        public string Category { get; set; }
        public int PreparationTime { get; set; }
        public string? ImageUrl { get; set; }

        public bool IsFavorite { get; set; }
    }
}
