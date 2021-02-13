using System;
using Meuowe.Areas.Database.Models.DatabaseObjects;
using Meuowe.Areas.Database.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace Meuowe.Areas.Database.Models
{
    public class MeuoweDbContext : IdentityDbContext
    {
        public MeuoweDbContext(DbContextOptions<MeuoweDbContext> options)
            : base(options)
        {
        }

        // Steps to add new database object relation with table.
        // 1) dotnet ef migrations add <Enter Description>
        // 2) dotnet ef database update
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<UserDefaultImageDBO> UserDefaultImages { get; set; }
        public DbSet<UserSettingsDBO> UserSettings { get; set; }
        public DbSet<UserPawDBO> UserPaws { get; set; }
        public DbSet<UserMeuoweDBO> UserMeuowes { get; set; }
        public DbSet<UserPurrDBO> UserPurrs { get; set; }
        public DbSet<UserFollowDBO> UserFollows { get; set; }
        public DbSet<UserShakeDBO> UserShakes { get; set; }
        public DbSet<UserWagDBO> UserWags { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Renames default identity tables.
            // 1) delete migrations folder and db
            // 2*) dotnet aspnet-codegenerator identity -dc Meuowe.Areas.Database.Models.ApplicationDbContext
            // 2) dotnet ef migrations add CreateIdentitySchema
            // 4) dotnet ef database update
            base.OnModelCreating(builder);
            builder.Entity<IdentityUser>().ToTable("Users");
            builder.Entity<IdentityRole>().ToTable("Roles");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
        }
    }
}
