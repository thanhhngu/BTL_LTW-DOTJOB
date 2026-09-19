using BTL_LTW_DOTJOB.Models;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTW_DOTJOB.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Company> Companies { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Company)       // 1 User có 1 Company
                .WithOne(c => c.User)         // 1 Company thuộc về 1 User
                .HasForeignKey<Company>(c => c.UserId) // Khóa ngoại nằm ở bảng Company
                .OnDelete(DeleteBehavior.Cascade);     // Xóa User thì tự động xóa luôn Company
        }

    }
}
