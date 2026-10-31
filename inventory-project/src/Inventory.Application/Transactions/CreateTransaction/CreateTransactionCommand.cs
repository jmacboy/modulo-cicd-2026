using Joseco.DDD.Core.Results;
using MediatR;

namespace Inventory.Application.Transactions.CreateTransaction;

//public record CreateTransactionCommand(Guid UserCreatorId, string Type, 
//ICollection<CreateTransacionItemCommand> Items, 
//Guid? SourceId = null, string? SourceType = null) : IRequest<Result<Guid>>;

public record CreateTransactionCommand : IRequest<Result<Guid>>
{
    public Guid UserCreatorId { get; init; }
    public string Type { get; init; }
    public ICollection<CreateTransacionItemCommand> Items { get; init; }
    public Guid? SourceId { get; init; } = null;
    public string? SourceType { get; init; } = null;
}

public record CreateTransacionItemCommand(Guid ItemId, int Quantity, decimal UnitaryCost);
