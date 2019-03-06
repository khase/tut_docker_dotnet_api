using Microsoft.EntityFrameworkCore;

namespace DockerDotnetApiTutorial.Api.Data
{
    public class MyDbContext: DbContext
    {
        public MyDbContext(DbContextOptions options): base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ForNpgsqlUseIdentityAlwaysColumns();
        }

        public DbSet<Person> Persons { get;set; }
    }
}