using System.ComponentModel.DataAnnotations;

namespace WebClient.DTOs.Request
{
    public class CreateCategoryDTO
    {

        [Required]
        public string CategoryName { get; set; } = null!;

        public string? Description { get; set; }

        public bool? IsActive { get; set; }
    }

    public class UpdateCategoryDTO
    {
        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = null!;

        public string? Description { get; set; }

        public bool? IsActive { get; set; }
    }
}
