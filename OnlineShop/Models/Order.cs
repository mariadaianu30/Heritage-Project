using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineShop.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "Adresa de livrare este obligatorie")]
        [StringLength(500, ErrorMessage = "Adresa nu poate depăși 500 de caractere")]
        public string ShippingAddress { get; set; } = null!;

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        // ===== CHEI STRĂINE =====

        [Required]
        public string UserId { get; set; } = null!;

        // ===== RELAȚII DE NAVIGARE =====

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        // Produsele din comandă (1:N)
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // ===== PROPRIETĂȚI CALCULATE =====

        [NotMapped]
        public int TotalProducts => OrderItems?.Sum(oi => oi.Quantity) ?? 0;

        // ===== METODE HELPER =====

        // Calculează totalul comenzii din OrderItems
        public void CalculateTotal()
        {
            TotalAmount = OrderItems?.Sum(oi => oi.Subtotal) ?? 0;
        }
    }
}