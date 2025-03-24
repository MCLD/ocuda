using System.Linq;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Data
{
    public class BooksByMailContext : DbContext
    {
        public BooksByMailContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // turn off cascading deletes
            foreach (var relationship in modelBuilder.Model
                .GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }

        public void Migrate()
        {
            Database.Migrate();
        }

        public DbSet<BooksByMailComment> Comments { get; set; }
        public DbSet<BooksByMailCustomer> Customers { get; set; }
    }
}
