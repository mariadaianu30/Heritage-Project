using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Câmpuri adiționale pentru utilizator
        [StringLength(50)]
        public string? FirstName { get; set; }

        [StringLength(50)]
        public string? LastName { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Proprietate calculată pentru numele complet
        public string FullName => $"{FirstName} {LastName}".Trim();


        // Coșul de cumpărături al utilizatorului (1:1)
        public virtual Cart? Cart { get; set; }

        // Wishlist-ul utilizatorului (1:1)
        public virtual Wishlist? Wishlist { get; set; }

        // Comenzile utilizatorului (1:N)
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

        // Review-urile scrise de utilizator (1:N)
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

        // Produsele propuse de utilizator - pentru colaboratori (1:N)
        public virtual ICollection<Product> ProposedProducts { get; set; } = new List<Product>();
    }
}