using Microsoft.EntityFrameworkCore;

namespace Examples.Repository.EFCoreRepositoryTests.Entities
{
    public class PeopleDbContext : DbContext
    {
        public PeopleDbContext()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            const string connectionString = "data source=.\\SQLEXPRESS;initial catalog=TestPeopleDb;integrated security=SSPI";
            optionsBuilder.UseSqlServer(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Person> People { get; set; }
    }
}