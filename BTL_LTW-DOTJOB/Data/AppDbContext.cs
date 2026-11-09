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
        public DbSet<SavedJob> SavedJobs { get; set; }
        public DbSet<Report> Reports { get; set; }

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

            // 3. Chống trùng dữ liệu ở mức Database (chặn cả trường hợp bấm gửi 2 lần cùng lúc)
            // Mỗi email chỉ có 1 tài khoản
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Mỗi Ứng viên chỉ nộp 1 đơn cho 1 tin tuyển dụng
            modelBuilder.Entity<Application>()
                .HasIndex(a => new { a.JobPostingId, a.UserId })
                .IsUnique();

            // 5. Bảng Report dùng chung (Candidate báo cáo tin, Employer báo cáo ứng viên, Admin xử lý).
            // Người báo cáo và người bị báo cáo đều trỏ về User => đặt Restrict để tránh Multiple Cascade Paths
            modelBuilder.Entity<Report>(entity =>
            {
                entity.HasOne(r => r.Reporter)
                    .WithMany()
                    .HasForeignKey(r => r.ReporterId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.TargetUser)
                    .WithMany()
                    .HasForeignKey(r => r.TargetUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.JobPosting)
                    .WithMany()
                    .HasForeignKey(r => r.JobPostingId)
                    .OnDelete(DeleteBehavior.Cascade); // Tin bị xóa thì báo cáo về tin đó cũng bị xóa

                // Danh sách của Admin lọc theo trạng thái và xếp theo thời gian
                entity.HasIndex(r => new { r.Status, r.CreatedAt });

                // Mỗi báo cáo trỏ tới đúng 1 đối tượng, khớp với TargetType
                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_Report_Target",
                    "([TargetType] = 'JobPosting' AND [JobPostingId] IS NOT NULL AND [TargetUserId] IS NULL) " +
                    "OR ([TargetType] = 'User' AND [TargetUserId] IS NOT NULL AND [JobPostingId] IS NULL)"));
            });

            // Không cho xóa Danh mục nếu vẫn còn tin tuyển dụng thuộc danh mục đó
            // (mặc định cũ là Cascade: xóa danh mục sẽ xóa luôn toàn bộ tin + đơn ứng tuyển)
            modelBuilder.Entity<JobPosting>()
                .HasOne(j => j.Category)
                .WithMany(c => c.JobPostings)
                .HasForeignKey(j => j.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Tên danh mục không được trùng nhau (chặn ở mức Database)
            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique();



            // 4. Tin đã lưu: chặn Multiple Cascade Paths (User -> Company -> JobPosting -> SavedJob và User -> SavedJob)
            modelBuilder.Entity<SavedJob>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SavedJob>()
                .HasOne(s => s.JobPosting)
                .WithMany()
                .HasForeignKey(s => s.JobPostingId)
                .OnDelete(DeleteBehavior.Cascade); // Tin bị xóa thì các lượt lưu tin đó cũng bị xóa

            // Mỗi Ứng viên chỉ lưu 1 tin 1 lần
            modelBuilder.Entity<SavedJob>()
                .HasIndex(s => new { s.UserId, s.JobPostingId })
                .IsUnique();
        }

    }
}
