using YemekTarifAPI.Models;

namespace YemekTarifAPI.Data
{
    public static class FakeRecipeData
    {
        public static List<Recipe> Recipes = new List<Recipe>
        {
            new Recipe
            {
                Id = Guid.NewGuid(),
                Name = "Menemen",
                Description = "Domatesli, yumurtalı kahvaltılık yemek.",
                Ingredients = "Yumurta, Domates, Biber, Tuz",
                Steps = "1. Biberi kavur. 2. Domatesi ekle. 3. Yumurtayı kır.",
                Category = "Kahvaltı",
                PreparationTime = 10,
                ImageUrl = "https://example.com/menemen.jpg"
            }
        };
        public static List<Comment> Comments = new List<Comment>();

    }
}
