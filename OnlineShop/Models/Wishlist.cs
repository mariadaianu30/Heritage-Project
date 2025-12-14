using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineShop.Models
{
    public class Wishlist
    {
        [Key]
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ===== CHEI STRĂINE =====

        [Required]
        public string UserId { get; set; } = null!;

        // ===== RELAȚII DE NAVIGARE =====

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        // Produsele din wishlist (1:N)
        public virtual ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();

        // ===== PROPRIETĂȚI CALCULATE =====

        [NotMapped]
        public int TotalItems => WishlistItems?.Count ?? 0;
    }
}