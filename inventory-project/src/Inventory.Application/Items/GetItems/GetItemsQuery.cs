using Joseco.DDD.Core.Results;
using MediatR;

namespace Inventory.Application.Items.GetItems;

public record GetItemsQuery(string? searchTerm = null, int? stockMin = null, int? stockMax = null) :
    IRequest<Result<ICollection<ItemDto>>>;