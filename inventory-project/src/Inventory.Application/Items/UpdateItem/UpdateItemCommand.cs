using Joseco.DDD.Core.Results;
using MediatR;

namespace Inventory.Application.Items.UpdateItem;

public record UpdateItemCommand(Guid Id, string Name) : IRequest<Result<bool>>;