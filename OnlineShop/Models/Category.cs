using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineShop.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Numele categoriei este obligatoriu")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Numele trebuie să aibă între 2 și 100 de caractere")]
        public string Name { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Descrierea nu poate depăși 500 de caractere")]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? ImagePath { get; set; }

        // ===== RELAȚII DE NAVIGARE =====

        // Produsele din această categorie (1:N)
        // La ștergerea categoriei se vor șterge toate produsele (Cascade Delete)
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}