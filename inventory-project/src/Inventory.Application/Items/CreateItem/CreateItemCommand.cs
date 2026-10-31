using Joseco.DDD.Core.Results;
using MediatR;

namespace Inventory.Application.Items.CreateItem;


public record CreateItemCommand(Guid Id, string ItemName) : IRequest<Result<Guid>>;