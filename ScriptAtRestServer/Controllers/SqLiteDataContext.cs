using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ScriptAtRestServer.Entities;

namespace ScriptAtRestServer.Controllers
{
    public class SqLiteDataContext : DbContext
    {
        protected readonly IConfiguration Configuration;

        public SqLiteDataContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // connect to sql server database
            options.UseSqlite(Configuration.GetConnectionString("SqLiteDatabase"));
        }

        public DbSet<User> Users { get; set; }
    }
}