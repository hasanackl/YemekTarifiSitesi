using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YemekTarifAPI.Data;
using YemekTarifAPI.DTOs;
using YemekTarifAPI.Models;

namespace YemekTarifAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FavoritesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FavoritesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult AddToFavorites([FromBody] FavoriteRecipeDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var alreadyExists = _context.FavoriteRecipes
                .Any(f => f.UserId == userId && f.RecipeId == dto.RecipeId);

            if (alreadyExists)
                return BadRequest("Tarif zaten favorilere eklenmiş.");

            var favorite = new FavoriteRecipe
            {
                UserId = userId,
                RecipeId = dto.RecipeId
            };

            _context.FavoriteRecipes.Add(favorite);
            _context.SaveChanges();

            return Ok("Favorilere eklendi.");
        }

        [HttpGet]
        public IActionResult GetFavorites()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var favorites = _context.FavoriteRecipes
                .Where(f => f.UserId == userId)
                .Select(f => f.RecipeId)
                .ToList();

            return Ok(favorites);
        }

        [HttpDelete("{recipeId}")]
        public IActionResult RemoveFromFavorites(Guid recipeId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var favorite = _context.FavoriteRecipes
                .FirstOrDefault(f => f.UserId == userId && f.RecipeId == recipeId);

            if (favorite == null)
                return NotFound("Favoride böyle bir tarif yok.");

            _context.FavoriteRecipes.Remove(favorite);
            _context.SaveChanges();

            return Ok("Favorilerden çıkarıldı.");
        }
    }
}
