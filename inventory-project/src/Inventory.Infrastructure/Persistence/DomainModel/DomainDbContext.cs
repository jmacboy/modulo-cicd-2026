using Inventory.Domain.Items;
using Inventory.Domain.Transactions;
using Inventory.Domain.Transactions.Events;
using Inventory.Domain.Users;
using Joseco.DDD.Core.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Inventory.Infrastructure.Persistence.DomainModel
{
    internal class DomainDbContext : DbContext
    {
        public DbSet<Item> Item { get; set; }
        public DbSet<Transaction> Transaction { get; set; }
        public DbSet<TransactionItem> TransactionItem { get; set; }
        public DbSet<User> User { get; set; }

        public DomainDbContext(DbContextOptions<DomainDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);

            modelBuilder.Ignore<DomainEvent>();
            modelBuilder.Ignore<TransactionCompleted>();
        }
    }
}
