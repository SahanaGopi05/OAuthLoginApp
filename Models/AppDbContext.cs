using Microsoft.EntityFrameworkCore;

namespace GoogleOAuthDemo.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<TestResult> TestResults { get; set; }
    }
}
