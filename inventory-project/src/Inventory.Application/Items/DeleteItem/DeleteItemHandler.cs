using Inventory.Domain.Items;
using Joseco.DDD.Core.Abstractions;
using Joseco.DDD.Core.Results;
using MediatR;

namespace Inventory.Application.Items.DeleteItem;

public class DeleteItemHandler : IRequestHandler<DeleteItemCommand, Result<bool>>
{
    private readonly IItemRepository _itemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteItemHandler(IItemRepository itemRepository, IUnitOfWork unitOfWork)
    {
        _itemRepository = itemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteItemCommand request, CancellationToken cancellationToken)
    {
        var existingItem = await _itemRepository.GetByIdAsync(request.Id, readOnly: true);

        if (existingItem == null)
        {
            return Result.Failure<bool>(Error.NotFound("ItemNotFound", "Item with id {itemId} not found", request.Id.ToString()));
        }

        await _itemRepository.DeleteAsync(request.Id);

        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(true);
    }
}
