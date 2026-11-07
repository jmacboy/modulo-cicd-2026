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

    public static Error ItemAlreadyDeleted() =>
        new Error("ItemAlreadyDeleted", "The item has already been deleted", ErrorType.Validation);

    public static Error CannotDeleteWithReservedStock(int reservedStock) =>
        new Error("CannotDeleteWithReservedStock", "The item has {reservedStock} reserved units and cannot be deleted",
            ErrorType.Validation, reservedStock);

    public static Error CannotUpdateDeletedItem() =>
        new Error("CannotUpdateDeletedItem", "The item has been deleted and cannot be updated", ErrorType.Validation);
}