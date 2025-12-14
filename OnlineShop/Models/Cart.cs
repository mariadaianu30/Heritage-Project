using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineShop.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // ===== CHEI STRĂINE =====

        [Required]
        public string UserId { get; set; } = null!;

        // ===== RELAȚII DE NAVIGARE =====

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        // Produsele din coș (1:N)
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

        // ===== PROPRIETĂȚI CALCULATE =====

        // Total produse în coș
        [NotMapped]
        public int TotalItems => CartItems?.Sum(ci => ci.Quantity) ?? 0;

        // Valoarea totală a coșului
        [NotMapped]
        public decimal TotalPrice => CartItems?
            .Where(ci => ci.Product != null)
            .Sum(ci => ci.Quantity * ci.Product!.Price) ?? 0;
    }
}