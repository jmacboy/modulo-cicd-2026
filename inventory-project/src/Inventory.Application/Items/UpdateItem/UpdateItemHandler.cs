using Inventory.Domain.Items;
using Joseco.DDD.Core.Abstractions;
using Joseco.DDD.Core.Results;
using MediatR;

namespace Inventory.Application.Items.UpdateItem;

public class UpdateItemHandler : IRequestHandler<UpdateItemCommand, Result<bool>>
{
    private readonly IItemRepository _itemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateItemHandler(IItemRepository itemRepository, IUnitOfWork unitOfWork)
    {
        _itemRepository = itemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(UpdateItemCommand request, CancellationToken cancellationToken)
    {
        var existingItem = await _itemRepository.GetByIdAsync(request.Id);

        if (existingItem == null)
        {
            return Result.Failure<bool>(Error.NotFound("ItemNotFound", "Item with id {itemId} not found", request.Id.ToString()));
        }

        try
        {
            existingItem.Rename(request.Name);
        }
        catch (DomainException ex)
        {
            return Result.Failure<bool>(ex.Error);
        }

        await _itemRepository.UpdateAsync(existingItem);

        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(true);
    }
}