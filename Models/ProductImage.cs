using System.ComponentModel.DataAnnotations;

namespace Tuan3.Models
{
    public class ProductImage
    {
        public int Id { get; set; }

        [Required]
        public string Url { get; set; } = string.Empty;

        // Khóa ngoại trỏ đến bảng Product
        public int ProductId { get; set; }

        // Thuộc tính điều hướng ngược lại Product
        public Product? Product { get; set; }
    }
}
