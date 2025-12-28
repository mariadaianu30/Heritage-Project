using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OnlineShop.Models.Enums;

namespace OnlineShop.Models
{
    public class ProductMaterial
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public MaterialType Material { get; set; }

        [Range(1, 100, ErrorMessage = "Procentajul trebuie să fie între 1 și 100")]
        public int Percentage { get; set; }

        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; } = null!;
    }
}
