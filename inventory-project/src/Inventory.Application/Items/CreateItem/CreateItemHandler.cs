using Inventory.Domain.Items;
using Joseco.DDD.Core.Abstractions;
using Joseco.DDD.Core.Results;
using MediatR;

namespace Inventory.Application.Items.CreateItem;

public class CreateItemHandler : IRequestHandler<CreateItemCommand, Result<Guid>>
{
    private readonly IItemRepository _itemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateItemHandler(IItemRepository itemRepository, IUnitOfWork unitOfWork)
    {
        _itemRepository = itemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateItemCommand request, CancellationToken cancellationToken)
    {
        var item = new Item(request.Id, request.ItemName);

        await _itemRepository.AddAsync(item);

        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(item.Id);

    }
}
