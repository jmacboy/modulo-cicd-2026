using Joseco.DDD.Core.Results;
using MediatR;

namespace Inventory.Application.Items.DeleteItem;

public record DeleteItemCommand(Guid Id) : IRequest<Result<bool>>;
