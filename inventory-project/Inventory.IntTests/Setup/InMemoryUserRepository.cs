using Inventory.Domain.Users;
using System.Collections.Concurrent;

namespace Inventory.IntTests.Setup
{
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly ConcurrentDictionary<Guid, User> _users = new();

        public Task AddAsync(User entity)
        {
            if (!_users.TryAdd(entity.Id, entity))
            {
                throw new InvalidOperationException(
                    $"A user with id {entity.Id} already exists.");
            }

            return Task.CompletedTask;
        }

        public Task<User?> GetByIdAsync(Guid id, bool readOnly = false)
        {
            _users.TryGetValue(id, out var user);
            return Task.FromResult(user);
        }
    }
}
