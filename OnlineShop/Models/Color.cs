using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models
{
    public class Color
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = null!;

        // util pentru UI (opțional)
        [StringLength(7)]
        public string? HexCode { get; set; }

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
