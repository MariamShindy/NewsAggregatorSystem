using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using News.Core.Entities;

namespace News.Infrastructure.Data
{
	public class ApplicationDbContext :IdentityDbContext<ApplicationUser>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
		: base(options)
		{

		}
		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);
			var adminRoleId = Guid.NewGuid().ToString();
			var userRoleId = Guid.NewGuid().ToString();
            //if (!builder.Model.GetEntityTypes().Any(t => t.ClrType == typeof(IdentityRole)))
            //{
            //    builder.Entity<IdentityRole>().HasData(
            //        new IdentityRole
            //        {
            //            Id = adminRoleId,
            //            Name = "Admin",
            //            NormalizedName = "ADMIN"
            //        },
            //        new IdentityRole
            //        {
            //            Id = userRoleId,
            //            Name = "User",
            //            NormalizedName = "USER"
            //        }
            //    );
            //}
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = adminRoleId,
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                },
                new IdentityRole
                {
                    Id = userRoleId,
                    Name = "User",
                    NormalizedName = "USER"
                }
            );
        }

		public DbSet<Comment> Comments { get; set; }
		public DbSet<UserFavoriteArticle> UserFavoriteArticles { get; set; }
        public DbSet<Category> Categories { get; set; }

	}
}
