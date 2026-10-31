using Inventory.Application.Items.GetItems;
using Inventory.Infrastructure.Persistence.StoredModel;
using Joseco.DDD.Core.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Queries.Items;

internal class GetItemsHandler : IRequestHandler<GetItemsQuery, Result<ICollection<ItemDto>>>
{
    private readonly PersistenceDbContext _dbContext;

    public GetItemsHandler(PersistenceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ICollection<ItemDto>>> Handle(GetItemsQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.Item.AsNoTracking().
            Select(i => new ItemDto()
            {
                Id = i.Id,
                ItemName = i.ItemName,
                Available = i.Available,
                Reserved = i.Reserverd,
                Stock = i.Stock
            }).
            ToListAsync(cancellationToken);
    }
}