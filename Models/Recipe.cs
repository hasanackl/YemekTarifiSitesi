using System.ComponentModel.DataAnnotations;

namespace YemekTarifAPI.Models
{
    public class Recipe
    {
        public Guid Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [Required, StringLength(1000)]
        public string Description { get; set; }

        [Required]
        public string Ingredients { get; set; }

        [Required]
        public string Steps { get; set; }

        [Required, StringLength(50)]
        public string Category { get; set; }

        [Range(1, 1440)]
        public int PreparationTime { get; set; }

        [Url]
        public string? ImageUrl { get; set; }

        public ICollection<Comment> Comments { get; set; }
        public ICollection<FavoriteRecipe> FavoriteRecipes { get; set; }
    }
}
