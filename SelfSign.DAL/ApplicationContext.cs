

using Microsoft.EntityFrameworkCore;
using SelfSign.Common.Entities;

namespace SelfSign.DAL
{
    public class ApplicationContext : DbContext
    {

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
            Database.Migrate();
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
    }
}
