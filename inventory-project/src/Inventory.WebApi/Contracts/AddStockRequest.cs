namespace Inventory.WebApi.Contracts;

public record AddStockRequest(Guid UserCreatorId, int Quantity, decimal UnitaryCost);