using BTL_LTW_DOTJOB.Models;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTW_DOTJOB.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<JobPosting> JobPostings { get; set; }
        public DbSet<Application> Applications { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Cấu hình Quan hệ 1-1 giữa User và Company
            modelBuilder.Entity<User>()
                .HasOne(u => u.Company)
                .WithOne(c => c.User)
                .HasForeignKey<Company>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa User xoa luon company

            // 2. Chặn lỗi Multiple Cascade Paths ở bảng Application
            modelBuilder.Entity<Application>()
                .HasOne(a => a.User)
                .WithMany(u => u.Applications)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Tắt xóa dây chuyền: Không cho xóa Ứng viên nếu họ đã có Đơn ứng tuyển (hoặc ngược lại, bảo vệ dữ liệu ứng tuyển)

            modelBuilder.Entity<Application>()
                .HasOne(a => a.JobPosting)
                .WithMany(j => j.Applications)
                .HasForeignKey(a => a.JobPostingId)
                .OnDelete(DeleteBehavior.Cascade); // Nếu bài tuyển dụng bị xóa, tự động xóa các Đơn ứng tuyển thuộc về bài đó
        }

    }
}
