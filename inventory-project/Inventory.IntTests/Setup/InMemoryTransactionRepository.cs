using Inventory.Domain.Transactions;
using System.Collections.Concurrent;

namespace Inventory.IntTests.Setup
{
    public class InMemoryTransactionRepository : ITransactionRepository
    {
        private readonly ConcurrentDictionary<Guid, Transaction> _transactions = new();

        public Task AddAsync(Transaction entity)
        {
            if (!_transactions.TryAdd(entity.Id, entity))
            {
                throw new InvalidOperationException(
                    $"A transaction with id {entity.Id} already exists.");
            }

            return Task.CompletedTask;
        }

        public Task<Transaction?> GetByIdAsync(Guid id, bool readOnly = false)
        {
            _transactions.TryGetValue(id, out var transaction);
            return Task.FromResult(transaction);
        }

        public Task UpdateAsync(Transaction transaction)
        {
            _transactions[transaction.Id] = transaction;
            return Task.CompletedTask;
        }
    }
}
