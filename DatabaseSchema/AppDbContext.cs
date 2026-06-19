using DatabaseSchema.Entities;
using Microsoft.EntityFrameworkCore;

namespace DatabaseSchema;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.UserName).IsUnique();

            entity.Property(u => u.CreatedAt)
                .HasDefaultValueSql("now()");

            entity.HasIndex(e => new { e.UserName, e.Password });
        });
    }
}