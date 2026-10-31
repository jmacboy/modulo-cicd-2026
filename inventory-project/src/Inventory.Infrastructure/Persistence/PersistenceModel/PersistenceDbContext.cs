using Inventory.Infrastructure.Persistence.PersistenceModel.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence.StoredModel
{
    internal class PersistenceDbContext : DbContext, IDatabase
    {
        public DbSet<ItemPersistenceModel> Item { get; set; }
        public DbSet<TransactionPersistenceModel> Transaction { get; set; }
        public DbSet<TransactionItemStoredModel> TransactionItem { get; set; }
        public DbSet<UserPersistenceModel> User { get; set; }

        public PersistenceDbContext(DbContextOptions<PersistenceDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public void Migrate()
        {
            if (Database.IsRelational())
            {
                Database.Migrate();
            }
        }
    }
}
