using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NuvellAPI.Models.Domain;

namespace NuvellAPI.Data;

public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }
    public DbSet<UserRefreshTokens> UserRefreshTokens { get; set; }
    public DbSet<Workspace> Workspaces { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        List<IdentityRole<Guid>> roles = new List<IdentityRole<Guid>>
        {
            new IdentityRole<Guid>
            {
                Id = Guid.Parse("e4aec2e7-af26-4bf1-8a40-b4eb10f7838b"),
                Name = "Admin",
                NormalizedName = "ADMIN"
            },
            new IdentityRole<Guid>
            {
                Id = Guid.Parse("f137d6fb-1d31-4dad-99f0-4cf8414f1d18"),
                Name = "User",
                NormalizedName = "USER"
            },
        };
        modelBuilder.Entity<IdentityRole<Guid>>().HasData(roles);
    }
}