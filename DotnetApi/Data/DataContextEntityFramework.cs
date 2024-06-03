using Microsoft.EntityFrameworkCore;
using DotnetApi.Models;

namespace DotnetApi.Data
{
  public class DataContextEntityFramework : DbContext
  {
    private readonly IConfiguration _config;

    public DataContextEntityFramework(IConfiguration config)
    {
      _config = config;
    }

    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<UserSalary> UserSalary { get; set; }
    public virtual DbSet<UserJobInfo> UserJobInfo { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      if (!optionsBuilder.IsConfigured)
      {
        optionsBuilder
          .UseSqlServer(_config.GetConnectionString("DefaultConnection"),
            optionsBuilder => optionsBuilder.EnableRetryOnFailure());
      }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.HasDefaultSchema("AppSchema");

      modelBuilder.Entity<User>()
        .ToTable("Users", "AppSchema")
        .HasKey(u => u.UserId);

      modelBuilder.Entity<UserSalary>()
        .HasKey(u => u.UserId);

      modelBuilder.Entity<User>()
        .HasKey(u => u.UserId);
    }
  }
}