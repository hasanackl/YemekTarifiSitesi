using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using YemekTarifAPI.Data;
using YemekTarifAPI.DTOs;
using YemekTarifAPI.Models;

namespace YemekTarifAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecipesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RecipesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET /api/recipes  -> isFavorite ile liste
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var favoriteIds = userId == null
                ? new HashSet<Guid>()
                : await _context.FavoriteRecipes
                    .Where(f => f.UserId == userId)
                    .Select(f => f.RecipeId)
                    .ToHashSetAsync();

            var recipes = await _context.Recipes
                .Select(r => new RecipeResponse
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    Ingredients = r.Ingredients,
                    Steps = r.Steps,
                    Category = r.Category,
                    PreparationTime = r.PreparationTime,
                    ImageUrl = r.ImageUrl,
                    IsFavorite = favoriteIds.Contains(r.Id)
                })
                .ToListAsync();

            return Ok(recipes);
        }

        // GET /api/recipes/{id} -> tek kayıt + isFavorite
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null) return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isFav = userId != null &&
                        await _context.FavoriteRecipes.AnyAsync(f => f.UserId == userId && f.RecipeId == id);

            var dto = new RecipeResponse
            {
                Id = recipe.Id,
                Name = recipe.Name,
                Description = recipe.Description,
                Ingredients = recipe.Ingredients,
                Steps = recipe.Steps,
                Category = recipe.Category,
                PreparationTime = recipe.PreparationTime,
                ImageUrl = recipe.ImageUrl,
                IsFavorite = isFav
            };

            return Ok(dto);
        }

        // GET /api/recipes/{id}/comments -> kullanıcı adıyla yorum listesi
        [HttpGet("{id}/comments")]
        public async Task<IActionResult> GetComments(Guid id)
        {
            var exists = await _context.Recipes.AnyAsync(r => r.Id == id);
            if (!exists) return NotFound("Recipe not found.");

            var comments = await _context.Comments
                .Where(c => c.RecipeId == id)
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new
                {
                    c.Id,
                    c.Text,
                    c.CreatedAt,
                    userName = c.User.UserName
                })
                .ToListAsync();

            return Ok(comments);
        }

        // GET /api/recipes/category/{category}
        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetByCategory(string category)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var favoriteIds = userId == null
                ? new HashSet<Guid>()
                : await _context.FavoriteRecipes
                    .Where(f => f.UserId == userId)
                    .Select(f => f.RecipeId)
                    .ToHashSetAsync();

            var cat = category.Trim().ToLower();
            var recipes = await _context.Recipes
                .Where(r => r.Category.ToLower() == cat)
                .Select(r => new RecipeResponse
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    Ingredients = r.Ingredients,
                    Steps = r.Steps,
                    Category = r.Category,
                    PreparationTime = r.PreparationTime,
                    ImageUrl = r.ImageUrl,
                    IsFavorite = favoriteIds.Contains(r.Id)
                })
                .ToListAsync();

            return Ok(recipes);
        }

        // GET /api/recipes/search?q=&category=&page=1&pageSize=10&sortBy=name&sortDir=asc
        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string? q,
            [FromQuery] string? category,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sortBy = "name",   // name | time
            [FromQuery] string sortDir = "asc")   // asc | desc
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var query = _context.Recipes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(r =>
                    r.Name.Contains(term) ||
                    r.Description.Contains(term) ||
                    r.Ingredients.Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                var cat = category.Trim().ToLower();
                query = query.Where(r => r.Category.ToLower() == cat);
            }

            bool desc = sortDir.Equals("desc", StringComparison.OrdinalIgnoreCase);
            query = (sortBy.ToLower()) switch
            {
                "time" => (desc ? query.OrderByDescending(r => r.PreparationTime) : query.OrderBy(r => r.PreparationTime)),
                _      => (desc ? query.OrderByDescending(r => r.Name)            : query.OrderBy(r => r.Name))
            };

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var favoriteIds = userId == null
                ? new HashSet<Guid>()
                : await _context.FavoriteRecipes
                    .Where(f => f.UserId == userId)
                    .Select(f => f.RecipeId)
                    .ToHashSetAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RecipeResponse
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    Ingredients = r.Ingredients,
                    Steps = r.Steps,
                    Category = r.Category,
                    PreparationTime = r.PreparationTime,
                    ImageUrl = r.ImageUrl,
                    IsFavorite = favoriteIds.Contains(r.Id)
                })
                .ToListAsync();

            var result = new PagedResult<RecipeResponse>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };

            return Ok(result);
        }

        // POST /api/recipes  -> sadece Admin
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Recipe recipe)
        {
            recipe.Id = Guid.NewGuid();
            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = recipe.Id }, recipe);
        }

        // POST /api/recipes/comment  -> login gerekli
        [Authorize]
        [HttpPost("comment")]
        public async Task<IActionResult> AddComment([FromBody] CommentDto commentDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var recipeExists = await _context.Recipes.FindAsync(commentDto.RecipeId);
            if (recipeExists == null) return NotFound("Recipe not found.");

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RecipeId = commentDto.RecipeId,
                Text = commentDto.Text,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return Ok(comment);
        }

        // PUT /api/recipes/{id}  -> sadece Admin
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Recipe updatedRecipe)
        {
            var existing = await _context.Recipes.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Name = updatedRecipe.Name;
            existing.Description = updatedRecipe.Description;
            existing.Ingredients = updatedRecipe.Ingredients;
            existing.Steps = updatedRecipe.Steps;
            existing.Category = updatedRecipe.Category;
            existing.PreparationTime = updatedRecipe.PreparationTime;
            existing.ImageUrl = updatedRecipe.ImageUrl;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE /api/recipes/{id}  -> sadece Admin
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null) return NotFound();

            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE /api/recipes/comment/{id} -> sahibi veya Admin
        [Authorize]
        [HttpDelete("comment/{id}")]
        public async Task<IActionResult> DeleteComment(Guid id)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id);
            if (comment == null) return NotFound("Comment not found.");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && comment.UserId != userId)
                return Forbid();

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
