using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models
{
    public class CollaboratorRequest
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = null!;

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public string? Status { get; set; } = "Pending"; // Pending, Approved, Rejected
    }
}
