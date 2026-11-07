using Joseco.DDD.Core.Results;

namespace Inventory.Domain.Transactions;

public static class TransactionErrors
{
    public static Error CreatorIsRequired() =>
        new Error("CreatorIsRequired", "The user creator id is required", ErrorType.Validation);
}