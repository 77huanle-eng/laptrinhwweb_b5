
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tuan3.Data;
using Tuan3.Models;
using Tuan3.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
})
    .AddDefaultTokenProviders()
    .AddDefaultUI()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<IProductRepository, EFProductRepository>();
builder.Services.AddScoped<ICategoryRepository, EFCategoryRepository>();

builder.Services.AddRazorPages();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapRazorPages();

app.MapControllerRoute(
    name: "Admin",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    if (!await roleManager.RoleExistsAsync(SD.Role_Admin))
    {
        await roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
        await roleManager.CreateAsync(new IdentityRole(SD.Role_Customer));
        await roleManager.CreateAsync(new IdentityRole(SD.Role_Company));
        await roleManager.CreateAsync(new IdentityRole(SD.Role_Employee));
    }

    var adminEmail = "admin@test.com";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Admin Test",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(adminUser, "Admin@123");
        await userManager.AddToRoleAsync(adminUser, SD.Role_Admin);
    }

    // Seed Categories
    if (!context.Categories.Any())
    {
        var categories = new List<Category>
        {
            new Category { Name = "Điện thoại" },
            new Category { Name = "Laptop" },
            new Category { Name = "Phụ kiện" }
        };
        context.Categories.AddRange(categories);
        context.SaveChanges();
    }

    // Seed Products
    if (!context.Products.Any())
    {
        var phoneCat = context.Categories.FirstOrDefault(c => c.Name == "Điện thoại");
        var laptopCat = context.Categories.FirstOrDefault(c => c.Name == "Laptop");
        var accessoryCat = context.Categories.FirstOrDefault(c => c.Name == "Phụ kiện");

        var products = new List<Product>
        {
            new Product
            {
                Name = "iPhone 15 Pro Max",
                Price = 29990000,
                Description = "Siêu phẩm iPhone 15 Pro Max với khung titan siêu bền, camera zoom 5x cực đỉnh.",
                ImageUrl = "https://images.unsplash.com/photo-1695048133142-1a20484d2569?q=80&w=600&auto=format&fit=crop",
                CategoryId = phoneCat?.Id ?? 1
            },
            new Product
            {
                Name = "Samsung Galaxy S24 Ultra",
                Price = 26990000,
                Description = "Flagship Samsung với tính năng Galaxy AI tiên tiến, bút S Pen đa năng.",
                ImageUrl = "https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?q=80&w=600&auto=format&fit=crop",
                CategoryId = phoneCat?.Id ?? 1
            },
            new Product
            {
                Name = "MacBook Air M3",
                Price = 27990000,
                Description = "MacBook Air mỏng nhẹ với vi xử lý M3 mạnh mẽ, pin dùng cả ngày.",
                ImageUrl = "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?q=80&w=600&auto=format&fit=crop",
                CategoryId = laptopCat?.Id ?? 2
            },
            new Product
            {
                Name = "ASUS ROG Zephyrus G14",
                Price = 35990000,
                Description = "Laptop gaming mỏng nhẹ, màn hình OLED 120Hz siêu đẹp, card đồ họa RTX 4060.",
                ImageUrl = "https://images.unsplash.com/photo-1603302576837-37561b2e2302?q=80&w=600&auto=format&fit=crop",
                CategoryId = laptopCat?.Id ?? 2
            },
            new Product
            {
                Name = "Tai nghe AirPods Pro 2",
                Price = 5990000,
                Description = "Tai nghe chống ồn chủ động đỉnh cao, âm thanh vòm cá nhân hóa tốt nhất.",
                ImageUrl = "https://images.unsplash.com/photo-1588449668365-d15e397f6787?q=80&w=600&auto=format&fit=crop",
                CategoryId = accessoryCat?.Id ?? 3
            },
            new Product
            {
                Name = "Chuột Logitech MX Master 3S",
                Price = 2490000,
                Description = "Chuột công thái học cao cấp dành cho lập trình viên và nhà thiết kế.",
                ImageUrl = "https://images.unsplash.com/photo-1615663245857-ac93bb7c39e7?q=80&w=600&auto=format&fit=crop",
                CategoryId = accessoryCat?.Id ?? 3
            }
        };
        context.Products.AddRange(products);
        context.SaveChanges();
    }
}

app.Run();
