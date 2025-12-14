using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineShop.Models
{
    public class ProductFAQ
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "Întrebarea nu poate depăși 500 de caractere")]
        public string Question { get; set; } = null!;

        [StringLength(1000, ErrorMessage = "Răspunsul nu poate depăși 1000 de caractere")]
        public string? Answer { get; set; }

        // Numărul de ori când a fost pusă această întrebare
        public int TimesAsked { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ===== CHEI STRĂINE =====

        [Required]
        public int ProductId { get; set; }

        // ===== RELAȚII DE NAVIGARE =====

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}