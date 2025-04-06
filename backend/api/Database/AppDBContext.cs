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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            List<IdentityRole> roles = new List<IdentityRole>
            {
                new IdentityRole { Name = "admin", NormalizedName = "ADMIN" },
                new IdentityRole { Name = "user", NormalizedName = "USER" },
            };
            builder.Entity<IdentityRole>().HasData(roles);
        }
    }
}