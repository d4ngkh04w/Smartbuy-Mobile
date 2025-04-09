using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace api.Database
{
    public class AppDBContext : IdentityDbContext<IdentityUser>
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }

        public DbSet<Brand> Brands { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        // public DbSet<Color> Colors { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole { Name = "admin", NormalizedName = "ADMIN" },
                new IdentityRole { Name = "user", NormalizedName = "USER" }
            );

            // Cấu hình mối quan hệ Brand - Category
            builder.Entity<Brand>()
                .HasMany(b => b.Categories)
                .WithOne(c => c.Brand)
                .HasForeignKey(c => c.BrandId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa thương hiệu sẽ xóa tất cả danh mục của nó

            // Cấu hình mối quan hệ Category - Product
            builder.Entity<Category>()
                .HasMany(c => c.Products)
                .WithOne(p => p.Category)
                .HasForeignKey(p => p.CategoryId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull); // Xóa danh mục sẽ không xóa sản phẩm, chỉ đặt CategoryId của sản phẩm thành null

            // Đảm bảo tên danh mục không trùng lặp trong cùng một thương hiệu
            builder.Entity<Category>()
                .HasIndex(c => new { c.Name, c.BrandId })
                .IsUnique();

            base.OnModelCreating(builder);
        }
    }
}