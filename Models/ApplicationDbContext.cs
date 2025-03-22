using Microsoft.EntityFrameworkCore;

namespace WeirdAdminPanel.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options){}

    public DbSet<User> User {get; set;}

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<User>(user =>
        {
            user.HasIndex(e => e.Email).IsUnique();
        });
    }
}
