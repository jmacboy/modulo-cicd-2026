using Inventory.Domain.Transactions;
using Joseco.DDD.Core.Abstractions;
using Joseco.DDD.Core.Results;
using MediatR;

namespace Inventory.Application.Transactions.CompleteTransaction;

internal class CompleteTransactionHandler : IRequestHandler<CompleteTransactionCommand, Result<bool>>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteTransactionHandler(ITransactionRepository transactionRepository,
        IUnitOfWork unitOfWork)
    {
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(CompleteTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _transactionRepository.GetByIdAsync(request.TransactionId);

        if (transaction == null)
        {
            return Result.Failure<bool>(Error.NotFound("Transaction.NotFound", "The transaction was not found"));
        }

        transaction.Complete();

        //Actuializar el inventario
        //Notificar a otros sistemas si es necesario
        //Enviar una notificaion al encargado de inventarios


        await _unitOfWork.CommitAsync(cancellationToken);

        return true;

    }
}