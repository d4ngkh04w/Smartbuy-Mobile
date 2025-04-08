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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            List<IdentityRole> roles = new List<IdentityRole>
            {
                new IdentityRole { Name = "admin", NormalizedName = "ADMIN" },
                new IdentityRole { Name = "user", NormalizedName = "USER" },
            };
            builder.Entity<IdentityRole>().HasData(roles);

            // Cấu hình mối quan hệ Brand - Category
            builder.Entity<Brand>()
                .HasMany(b => b.Categories)
                .WithOne(c => c.Brand)
                .HasForeignKey(c => c.BrandId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa thương hiệu sẽ xóa tất cả danh mục của nó

            // Đảm bảo tên danh mục không trùng lặp trong cùng một thương hiệu
            builder.Entity<Category>()
                .HasIndex(c => new { c.Name, c.BrandId })
                .IsUnique();
            
            base.OnModelCreating(builder);
        }
    }
}