using BTL_LTW_DOTJOB.Models;

namespace BTL_LTW_DOTJOB.Helpers
{
    public static class JobQueryExtensions
    {
        public static IQueryable<JobPosting> WhereOpen(this IQueryable<JobPosting> jobs)
        {
            var now = DateTime.Now;
            return jobs.Where(job => job.IsActive && (job.Deadline == null || job.Deadline >= now));
        }
    }
}
