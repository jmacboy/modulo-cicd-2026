using Inventory.Domain.Items;
using Inventory.Domain.Transactions;
using Inventory.Domain.Transactions.Events;
using Joseco.DDD.Core.Abstractions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventory.Application.Items.EventHandlers;

public class UpdateStockWhenTransactionCompleted :
    INotificationHandler<TransactionCompleted>
{
    private readonly IItemRepository _itemRepository;
    private readonly ICostStrategy _costStrategy;

    public UpdateStockWhenTransactionCompleted(IItemRepository itemRepository, ICostStrategy costStrategy)
    {
        _itemRepository = itemRepository;
        _costStrategy = costStrategy;
    }

    public async Task Handle(TransactionCompleted domainEvent, CancellationToken cancellationToken)
    {
        bool isEntry = domainEvent.TransactionType == TransactionType.Entry;
        foreach (var item in domainEvent.Details)
        {
            var itemEntity = await _itemRepository.GetByIdAsync(item.ItemId);
            if (itemEntity == null)
            {
                continue;
            }

            if (isEntry)
            {
                itemEntity.AddStock(item.Quantity, item.unitaryCost, _costStrategy);
            }
            else
            {
                itemEntity.ApplyReservation(item.Quantity);
            }
            await _itemRepository.UpdateAsync(itemEntity);
        }
    }
}
