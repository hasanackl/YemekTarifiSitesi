using System.ComponentModel.DataAnnotations;

namespace YemekTarifAPI.DTOs
{
    public class CommentDto
    {
        [Required]
        public Guid RecipeId { get; set; }

        [Required, StringLength(500)]
        public string Text { get; set; }
    }
}
