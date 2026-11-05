using Inventory.Domain.Items;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventory.IntTests.Setup
{
    public class InMemoryItemRepository : IItemRepository
    {
        private readonly ConcurrentDictionary<Guid, Item> _items = new();

        public Task AddAsync(Item entity)
        {
            if (!_items.TryAdd(entity.Id, entity))
            {
                throw new InvalidOperationException(
                    $"Ya existe un item con id {entity.Id}.");
            }

            return Task.CompletedTask;
        }

        // Mirrors the EF Core global query filter: soft-deleted items are invisible to reads.
        public Task<Item?> GetByIdAsync(Guid id, bool readOnly = false)
        {
            _items.TryGetValue(id, out var item);
            return Task.FromResult(item is { IsDeleted: false } ? item : null);
        }

        public Task UpdateAsync(Item item)
        {
            _items[item.Id] = item;
            return Task.CompletedTask;
        }
    }
}
