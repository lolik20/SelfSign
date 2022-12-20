

using Microsoft.EntityFrameworkCore;
using SelfSign.Common.Entities;
using EntityFrameworkCore.EncryptColumn;
using Microsoft.EntityFrameworkCore.DataEncryption;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore.DataEncryption.Providers;
using System.Text;

namespace SelfSign.DAL
{
    public class ApplicationContext : DbContext
    {

        private readonly IEncryptionProvider _provider;
        public ApplicationContext(DbContextOptions<ApplicationContext> options, IConfiguration configuration) : base(options)
        {
            byte[] key = Encoding.ASCII.GetBytes(configuration.GetSection("Encryption")["Key"]);
            byte[] vector = Encoding.ASCII.GetBytes(configuration.GetSection("Encryption")["Vector"]);
            _provider = new AesProvider(key, vector);
            Database.Migrate();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseEncryption(_provider);
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<History> History { get; set; }
        public DbSet<UserData> UserData { get; set; }
    }
}
