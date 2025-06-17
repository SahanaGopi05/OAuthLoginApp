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
        public DbSet<UploadedMaterial> UploadedMaterials { get; set; }
        public DbSet<User> Users { get; set; } // ✅ Added this line
    }
}
