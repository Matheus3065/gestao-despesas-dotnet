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

        public DbSet<Category> Categories { get; set; } = null!;
    }
}