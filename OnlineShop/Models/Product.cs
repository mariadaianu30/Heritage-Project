using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OnlineShop.Models.Enums;

namespace OnlineShop.Models
{
    public class Product
    {
        // ===== CHEIE PRIMARĂ =====
        [Key]
        public int Id { get; set; }

        // ===== DATE DE BAZĂ =====

        [Required(ErrorMessage = "Titlul produsului este obligatoriu")]
        [StringLength(200, MinimumLength = 3)]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Descrierea produsului este obligatorie")]
        [StringLength(2000, MinimumLength = 10)]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Imaginea produsului este obligatorie")]
        [StringLength(500)]
        public string ImagePath { get; set; } = null!;

        [Required]
        [Range(0.01, 1_000_000)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        // ===== ATRIBUTE FASHION =====

        [Required(ErrorMessage = "Mărimea este obligatorie")]
        public ProductSize Size { get; set; }

        [Required(ErrorMessage = "Culoarea este obligatorie")]
        public int ColorId { get; set; }

        [ForeignKey(nameof(ColorId))]
        public virtual Color Color { get; set; } = null!;

        // ===== RATING & STATUS =====

        [Column(TypeName = "decimal(3,2)")]
        public decimal? AverageRating { get; set; }

        public ProductStatus Status { get; set; } = ProductStatus.Pending;

        [StringLength(500)]
        public string? AdminFeedback { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // ===== CHEI STRĂINE =====

        [Required]
        public int CategoryId { get; set; }

        public string? CollaboratorId { get; set; }

        // ===== RELAȚII =====

        [ForeignKey(nameof(CategoryId))]
        public virtual Category? Category { get; set; }

        [ForeignKey(nameof(CollaboratorId))]
        public virtual ApplicationUser? Collaborator { get; set; }

        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<ProductFAQ> FAQs { get; set; } = new List<ProductFAQ>();
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // 🔥 materiale + procentaj
        public virtual ICollection<ProductMaterial> Materials { get; set; } = new List<ProductMaterial>();

        // ===== HELPER METHODS =====

        public bool IsAvailable =>
            Status == ProductStatus.Approved && Stock > 0;

        public bool AreMaterialsValid =>
            Materials.Any() && Materials.Sum(m => m.Percentage) == 100;

        public void RecalculateRating()
        {
            if (Reviews.Any(r => r.Rating.HasValue))
            {
                AverageRating = (decimal)Reviews
                    .Where(r => r.Rating.HasValue)
                    .Average(r => r.Rating!.Value);
            }
            else
            {
                AverageRating = null;
            }
        }
    }
}
