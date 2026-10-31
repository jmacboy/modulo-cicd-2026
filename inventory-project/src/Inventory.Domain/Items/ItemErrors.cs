using Joseco.DDD.Core.Results;

namespace Inventory.Domain.Items;

public static class ItemErrors
{
    public static Error InsuficentStockToSubstract(int currentStock, int requestedStock)
    {
        return new Error("InsuficentStockToSubstract", "The current stock is {currentStock}, it cannot substract {requestedStock}",
            ErrorType.Validation, currentStock, requestedStock);
    }

    public static Error InsuficentStockToReserve(int availableStock, int requestedStock)
    {
        return new Error("InsuficentStockToReserve", "The available stock is {availableStock}, it cannot reserve {requestedStock}",
            ErrorType.Validation, availableStock, requestedStock);
    }

    public static Error NonNegativeStock() =>
        new Error("NonNegativeStock", "The value for cannot be negative", ErrorType.Validation);

    public static Error CostStrategyNotProvided() =>
        new Error("CostStrategyNotProvided", "A cost strategy must be provided to calculate the new unitary cost",
            ErrorType.Validation);

    public static Error NameIsRequired() => new Error("NameIsRequired", "The item name is required", ErrorType.Validation);
}

