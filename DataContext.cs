using Microsoft.EntityFrameworkCore;

namespace WebApplication1
{
    public class DataContext : DbContext
    {
        public DbSet<AppUser> Users { get; set; }

        public DataContext(DbContextOptions options) : base(options) 
        { 

        }
    }
}
