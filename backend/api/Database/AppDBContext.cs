using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Database
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }

        public DbSet<Brand> Brands { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<ProductColor> Colors { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<ProductImage> ProductImages { get; set; } = null!;
        public DbSet<ProductDiscount> ProductDiscounts { get; set; } = null!;
        public DbSet<Discount> Discounts { get; set; } = null!;
        public DbSet<ProductDetail> ProductDetails { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>()
                .HasIndex(u => new { u.Email, u.PhoneNumber })
                .IsUnique();

            builder.Entity<Category>()
                .HasIndex(c => new { c.Name, c.BrandId })
                .IsUnique();

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


            // Cấu hình mối quan hệ Product - ProductColor
            builder.Entity<Product>()
                .HasMany(p => p.Colors)
                .WithOne(c => c.Product)
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình mối quan hệ Product - ProductImage
            builder.Entity<Product>()
                .HasMany(p => p.Images)
                .WithOne(i => i.Product)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình mối quan hệ Product - ProductDiscount
            builder.Entity<ProductDiscount>()
                .HasKey(pd => new { pd.ProductId, pd.DiscountId });

            // Cấu hình mối quan hệ Product - ProductDetail
            builder.Entity<Product>()
                .HasOne(p => p.Detail)
                .WithOne(d => d.Product)
                .HasForeignKey<ProductDetail>(d => d.Id)
                .OnDelete(DeleteBehavior.Cascade);

            // builder.Entity<Product>()
            //     .HasMany(p => p.Reviews)
            //     .WithOne(r => r.Product)
            //     .HasForeignKey(r => r.ProductId)
            //     .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(builder);
        }
    }
}