using Inventory.Domain.Items;
using Inventory.Domain.Transactions;
using Inventory.Domain.Transactions.Exceptions;
using Joseco.DDD.Core.Abstractions;
using Joseco.DDD.Core.Results;
using MediatR;

namespace Inventory.Application.Transactions.CreateTransaction;

internal class CreateTransactionHandler :
    IRequestHandler<CreateTransactionCommand, Result<Guid>>
{
    private readonly ITransactionFactory _transactionFactory;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IItemRepository _itemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTransactionHandler(ITransactionFactory transactionFactory,
        ITransactionRepository transactionRepository,
        IItemRepository itemRepository,
        IUnitOfWork unitOfWork)
    {
        _transactionFactory = transactionFactory;
        _transactionRepository = transactionRepository;
        _itemRepository = itemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        foreach (var item in request.Items)
        {
            var existingItem = await _itemRepository.GetByIdAsync(item.ItemId, readOnly: true);
            if (existingItem == null)
            {
                return Result.Failure<Guid>(Error.NotFound("ItemNotFound", "Item with id {itemId} not found", item.ItemId.ToString()));
            }

        }

        var transaction = request.Type switch
        {
            "Entry" => _transactionFactory.CreateEntryTransaction(
                request.UserCreatorId,
                [..request.Items.Select(i => (i.ItemId, i.Quantity, i.UnitaryCost))]
            ),
            "Exit" => _transactionFactory.CreateExitTransaction(
                request.UserCreatorId,
                [..request.Items.Select(i => (i.ItemId, i.Quantity))],
                request.SourceId,
                request.SourceType
            ),
            _ => throw new TransactionCreationException("Invalid transaction type")
        };

        await _transactionRepository.AddAsync(transaction);

        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(transaction.Id);

    }
}
