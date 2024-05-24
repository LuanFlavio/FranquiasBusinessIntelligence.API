using Domain.Data;
using Microsoft.EntityFrameworkCore;

namespace DataAccess
{
    public class FranquiasBIDbContext : DbContext
    {
        public FranquiasBIDbContext() : base() { }

        public DbSet<ICompany> Company { get; set; }
        public DbSet<IUser> User { get; set; }
        public DbSet<IBilling> Billing { get; set; }
        public DbSet<IPurchases> Purchases { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            //FRANQUIAS-BI
            builder.UseSqlServer("String Connection - SQL Server");
            base.OnConfiguring(builder);
        }
    }
}