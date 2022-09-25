using Microsoft.EntityFrameworkCore;
using SelfSign.Entities;

namespace SelfSign
{
    public class ApplicationContext:DbContext
    {

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
            Database.Migrate();
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Document> Documents { get; set; }

    }
}
