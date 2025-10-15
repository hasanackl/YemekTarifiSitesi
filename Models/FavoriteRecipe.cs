namespace YemekTarifAPI.Models
{
    public class FavoriteRecipe
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public Guid RecipeId { get; set; }
        public Recipe Recipe { get; set; }
    }
}
