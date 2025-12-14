using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineShop.Models
{
    public class WishlistItem
    {
        [Key]
        public int Id { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        // ===== CHEI STRĂINE =====

        [Required]
        public int WishlistId { get; set; }

        [Required]
        public int ProductId { get; set; }

        // ===== RELAȚII DE NAVIGARE =====

        [ForeignKey("WishlistId")]
        public virtual Wishlist? Wishlist { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}