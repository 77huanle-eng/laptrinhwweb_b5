using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Tuan3.Models; // Đảm bảo import namespace chứa các lớp Product, Category, ProductImage

namespace Tuan3.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        // Hàm khởi tạo nhận cấu hình (options) và truyền xuống lớp cha (DbContext)
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Định nghĩa các bảng (Tables) trong Cơ sở dữ liệu
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
    }
}