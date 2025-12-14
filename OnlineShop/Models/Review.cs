using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineShop.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        // Textul review-ului (opțional)
        [StringLength(1000, ErrorMessage = "Review-ul nu poate depăși 1000 de caractere")]
        public string? Content { get; set; }

        // Rating 1-5 (opțional, dar dacă e completat trebuie să fie între 1 și 5)
        [Range(1, 5, ErrorMessage = "Rating-ul trebuie să fie între 1 și 5")]
        public int? Rating { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // ===== CHEI STRĂINE =====

        [Required]
        public int ProductId { get; set; }

        [Required]
        public string UserId { get; set; } = null!;

        // ===== RELAȚII DE NAVIGARE =====

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}