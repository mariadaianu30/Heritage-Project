using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineShop.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Cantitatea trebuie să fie cel puțin 1")]
        public int Quantity { get; set; } = 1;

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        // ===== CHEI STRĂINE =====

        [Required]
        public int CartId { get; set; }

        [Required]
        public int ProductId { get; set; }

        // ===== RELAȚII DE NAVIGARE =====

        [ForeignKey("CartId")]
        public virtual Cart? Cart { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        // ===== PROPRIETĂȚI CALCULATE =====

        [NotMapped]
        public decimal Subtotal => Product != null ? Quantity * Product.Price : 0;
    }
}