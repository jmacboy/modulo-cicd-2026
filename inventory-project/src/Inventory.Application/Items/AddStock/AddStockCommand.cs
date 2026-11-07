using Joseco.DDD.Core.Results;
using MediatR;

namespace Inventory.Application.Items.AddStock;

public record AddStockCommand(Guid ItemId, Guid UserCreatorId, int Quantity, decimal UnitaryCost) : IRequest<Result<Guid>>;