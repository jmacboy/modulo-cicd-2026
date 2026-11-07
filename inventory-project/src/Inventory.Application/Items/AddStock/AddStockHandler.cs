using Inventory.Domain.Items;
using Inventory.Domain.Transactions;
using Inventory.Domain.Users;
using Joseco.DDD.Core.Abstractions;
using Joseco.DDD.Core.Results;
using MediatR;

namespace Inventory.Application.Items.AddStock;

public class AddStockHandler : IRequestHandler<AddStockCommand, Result<Guid>>
{
    private readonly IItemRepository _itemRepository;
    private readonly ITransactionFactory _transactionFactory;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddStockHandler(IItemRepository itemRepository,
        ITransactionFactory transactionFactory,
        ITransactionRepository transactionRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _itemRepository = itemRepository;
        _transactionFactory = transactionFactory;
        _transactionRepository = transactionRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(AddStockCommand request, CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0)
        {
            return Result.Failure<Guid>(ItemErrors.NonNegativeStock());
        }

        if (request.UnitaryCost < 0)
        {
            return Result.Failure<Guid>(ItemErrors.NonNegativeCost());
        }

        if (request.UserCreatorId == Guid.Empty)
        {
            return Result.Failure<Guid>(TransactionErrors.CreatorIsRequired());
        }

        var existingItem = await _itemRepository.GetByIdAsync(request.ItemId, readOnly: true);

        if (existingItem == null)
        {
            return Result.Failure<Guid>(Error.NotFound("ItemNotFound", "Item with id {itemId} not found", request.ItemId.ToString()));
        }

        var creator = await _userRepository.GetByIdAsync(request.UserCreatorId, readOnly: true);

        if (creator == null)
        {
            return Result.Failure<Guid>(TransactionErrors.CreatorNotFound());
        }

        var transaction = _transactionFactory.CreateEntryTransaction(
            request.UserCreatorId,
            [(request.ItemId, request.Quantity, request.UnitaryCost)]);

        transaction.Complete();

        await _transactionRepository.AddAsync(transaction);

        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(transaction.Id);
    }
}