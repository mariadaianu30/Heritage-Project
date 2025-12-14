using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineShop.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Cantitatea trebuie să fie cel puțin 1")]
        public int Quantity { get; set; }

        // Prețul unitar la momentul comenzii (salvăm pentru istoric)
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        // ===== CHEI STRĂINE =====

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        // ===== RELAȚII DE NAVIGARE =====

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        // ===== PROPRIETĂȚI CALCULATE =====

        [NotMapped]
        public decimal Subtotal => Quantity * UnitPrice;
    }
}