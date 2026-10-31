using Inventory.Domain.Transactions;
using Inventory.Domain.Transactions.Events;
using Inventory.Infrastructure.Persistence.DomainModel;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence.Respositories;

internal class TransactionRepository : ITransactionRepository
{
    private readonly DomainDbContext _dbContext;

    public TransactionRepository(DomainDbContext domainDbContext)
    {
        _dbContext = domainDbContext;
    }

    public async Task AddAsync(Transaction obj)
    {
        await _dbContext.Transaction.AddAsync(obj);
    }

    public async Task DeleteAsync(Guid id)
    {
        var obj = await GetByIdAsync(id);
        if (obj != null)
            _dbContext.Transaction.Remove(obj);
    }

    public async Task<Transaction?> GetByIdAsync(Guid id, bool readOnly = false)
    {
        if (readOnly)
        {
            return await _dbContext.Transaction.AsNoTracking().Include("_items").FirstOrDefaultAsync(i => i.Id == id);
        }
        else
        {
            return await _dbContext.Transaction
                .Include("_items").FirstOrDefaultAsync(i => i.Id == id);
        }
    }
    /*
     
     p => C1, C2, C3
     
     */
    public Task UpdateAsync(Transaction transaction)
    {
        var added = transaction.DomainEvents.Where(e => e is TransactionItemAdded)
            .Select(e => (TransactionItemAdded)e)
            .ToList();
        foreach (var e in added)
        {
            var itemToAdd = transaction.Items.First(i => i.ItemId == e.ItemId);
            _dbContext.TransactionItem.Add(itemToAdd);
        }


        _dbContext.Transaction.Update(transaction);
        return Task.CompletedTask;
    }
}
