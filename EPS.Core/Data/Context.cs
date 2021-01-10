using System.Data.Entity;
using System.Configuration;
using System.Data.Common;
using System;

namespace EPS.Data
{
    public class EPSContext : DbContext
    {
        public EPSContext() : base(GetConnection(), false)
        {
        }

        public DbSet<CardCode> CardCodes { get; set; }
        public DbSet<Product> Products { get; set; }

        public static DbConnection GetConnection()
        {
            var connection = ConfigurationManager.ConnectionStrings["EPSContext"];
            var factory = DbProviderFactories.GetFactory(connection.ProviderName);
            var dbCon = factory.CreateConnection();
            dbCon.ConnectionString = connection.ConnectionString;
            return dbCon;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
