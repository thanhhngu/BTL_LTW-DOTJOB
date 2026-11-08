using BTL_LTW_DOTJOB.Models;

namespace BTL_LTW_DOTJOB.Helpers
{
    public static class JobQueryExtensions
    {
        // Tin đang tuyển: được bật hiển thị (IsActive) và chưa quá hạn nộp hồ sơ.
        // Deadline = null nghĩa là không thời hạn. Hạn do HR chọn theo giờ máy (23:59:59 của ngày hạn),
        // nên so với DateTime.Now giống ApplicationService, không dùng UTC
        public static IQueryable<JobPosting> WhereOpen(this IQueryable<JobPosting> jobs)
        {
            var now = DateTime.Now;
            return jobs.Where(job => job.IsActive && (job.Deadline == null || job.Deadline >= now));
        }
    }
}
