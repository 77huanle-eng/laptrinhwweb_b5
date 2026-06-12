using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tuan3.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        [Range(0.01, 999999999999.00, ErrorMessage = "Giá bán phải nằm trong khoảng từ 0.01 đến 999,999,999,999 đ")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public List<ProductImage>? Images { get; set; } // Sẽ hết báo lỗi đỏ sau khi thêm ProductImage.cs
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}