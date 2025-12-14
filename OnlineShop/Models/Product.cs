using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineShop.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Titlul produsului este obligatoriu")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Titlul trebuie să aibă între 3 și 200 de caractere")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Descrierea produsului este obligatorie")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Descrierea trebuie să aibă între 10 și 2000 de caractere")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Imaginea produsului este obligatorie")]
        [StringLength(500)]
        public string ImagePath { get; set; } = null!;

        [Required(ErrorMessage = "Prețul este obligatoriu")]
        [Range(0.01, 1000000, ErrorMessage = "Prețul trebuie să fie mai mare decât 0")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stocul este obligatoriu")]
        [Range(0, int.MaxValue, ErrorMessage = "Stocul nu poate fi negativ")]
        public int Stock { get; set; }

        // Rating-ul mediu calculat automat din review-uri (1-5)
        [Column(TypeName = "decimal(3,2)")]
        public decimal? AverageRating { get; set; }

        // Statusul produsului (Pending, Approved, Rejected)
        public ProductStatus Status { get; set; } = ProductStatus.Pending;

        // Feedback de la administrator pentru colaborator
        [StringLength(500)]
        public string? AdminFeedback { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // ===== CHEI STRĂINE =====

        [Required(ErrorMessage = "Categoria este obligatorie")]
        public int CategoryId { get; set; }

        // Colaboratorul care a propus produsul (poate fi null pentru produse adăugate de admin)
        public string? CollaboratorId { get; set; }

        // ===== RELAȚII DE NAVIGARE =====

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        [ForeignKey("CollaboratorId")]
        public virtual ApplicationUser? Collaborator { get; set; }

        // Review-urile produsului (1:N)
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

        // FAQ-uri pentru AI Assistant (1:N)
        public virtual ICollection<ProductFAQ> FAQs { get; set; } = new List<ProductFAQ>();

        // Prezența în coșuri (1:N)
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

        // Prezența în wishlist-uri (1:N)
        public virtual ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();

        // Prezența în comenzi (1:N)
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // ===== METODE HELPER =====

        // Verifică dacă produsul este disponibil pentru vânzare
        public bool IsAvailable => Status == ProductStatus.Approved && Stock > 0;

        // Recalculează rating-ul mediu
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