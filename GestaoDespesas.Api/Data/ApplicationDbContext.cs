using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GestaoDespesas.Api.Models;

namespace GestaoDespesas.Api.Data

// The main database context for the application.
// Inherits from IdentityDbContext to automatically include
// the tables required by ASP.NET Core Identity (Users, Roles, etc.)
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; } 
        public DbSet<Expense> Expenses { get; set; } 

        // Additional EF Core configuration that goes beyond simple property attributes
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Explicitly define precision and scale for monetary values:
    // 18 total digits, 2 of which are after the decimal point (e.g. 123456789012345.67)
    modelBuilder.Entity<Expense>()
        .Property(e => e.Amount)
        .HasPrecision(18, 2);
}
    }
}