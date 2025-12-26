using System.ComponentModel.DataAnnotations;

namespace DistributedOrderSystem.DTOs
{
    public class ProductCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        [Range(0.01, 100000)]
        public decimal Price { get; set; }
        [Range(0, 10000)]
        public int Stock { get; set; }
    }
}
