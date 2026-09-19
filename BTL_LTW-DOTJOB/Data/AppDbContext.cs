using Microsoft.EntityFrameworkCore;

namespace BTL_LTW_DOTJOB.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    }
}
