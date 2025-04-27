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
        public DbSet<ProductLine> ProductLines { get; set; } = null!;
        public DbSet<ProductColor> Colors { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<ProductImage> ProductImages { get; set; } = null!;
        public DbSet<ProductDiscount> ProductDiscounts { get; set; } = null!;
        public DbSet<Discount> Discounts { get; set; } = null!;
        public DbSet<ProductDetail> ProductDetails { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!;
        public DbSet<ProductTag> ProductTags { get; set; } = null!;
        public DbSet<Cart> Carts { get; set; } = null!;
        public DbSet<CartItem> CartItems { get; set; } = null!;
        public DbSet<CarouselImage> CarouselImages { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>()
                .HasIndex(u => new { u.Email, u.PhoneNumber })
                .IsUnique();

            builder.Entity<ProductLine>()
                .HasIndex(pl => new { pl.Name, pl.BrandId })
                .IsUnique();

            // Cấu hình mối quan hệ Brand - ProductLine
            builder.Entity<Brand>()
                .HasMany(b => b.ProductLines)
                .WithOne(pl => pl.Brand)
                .HasForeignKey(pl => pl.BrandId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa thương hiệu sẽ xóa tất cả dòng sản phẩm của nó

            // Cấu hình mối quan hệ ProductLine - Product
            builder.Entity<ProductLine>()
                .HasMany(pl => pl.Products)
                .WithOne(p => p.ProductLine)
                .HasForeignKey(p => p.ProductLineId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa dòng sản phẩm sẽ xóa tất cả các sản phẩm thuộc dòng đó

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
                .HasForeignKey<ProductDetail>(d => d.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình mối quan hệ Product - Tag (nhiều-nhiều)
            builder.Entity<ProductTag>()
                .HasKey(pt => new { pt.ProductId, pt.TagId });

            builder.Entity<ProductTag>()
                .HasOne(pt => pt.Product)
                .WithMany(p => p.ProductTags)
                .HasForeignKey(pt => pt.ProductId);

            builder.Entity<ProductTag>()
                .HasOne(pt => pt.Tag)
                .WithMany(t => t.ProductTags)
                .HasForeignKey(pt => pt.TagId);

            builder.Entity<CarouselImage>()
                .HasKey(c => c.Id);


            base.OnModelCreating(builder);
        }
    }
}