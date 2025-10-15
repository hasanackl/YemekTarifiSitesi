using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YemekTarifAPI.Data;
using YemekTarifAPI.DTOs;
using YemekTarifAPI.Models;

namespace YemekTarifAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CommentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost]
        public IActionResult AddComment([FromBody] CommentDto commentDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                RecipeId = commentDto.RecipeId,
                UserId = userId,
                Text = commentDto.Text,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            _context.SaveChanges();

            return Ok(comment);
        }

        [HttpGet("{recipeId}")]
        public IActionResult GetComments(Guid recipeId)
        {
            var comments = _context.Comments
                .Where(c => c.RecipeId == recipeId)
                .Select(c => new
                {
                    c.Text,
                    c.CreatedAt,
                    c.UserId
                })
                .ToList();

            return Ok(comments);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var comment = _context.Comments.FirstOrDefault(c => c.Id == id);
            if (comment == null)
                return NotFound();

            _context.Comments.Remove(comment);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
