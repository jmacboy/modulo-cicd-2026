using Inventory.Application.Transactions.CompleteTransaction;
using Inventory.Domain.Transactions;
using Joseco.DDD.Core.Abstractions;
using Joseco.DDD.Core.Results;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Inventory.Tests.Application.Transactions.CompleteTransaction
{
    public class CompleteTransactionHandlerTest
    {
        [Fact]
        public async Task Handle_TransactionNotFound_ReturnsNotFoundFailure()
        {
            //Arrange
            var transactionRepositoryMock = new Mock<ITransactionRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var handler = new CompleteTransactionHandler(transactionRepositoryMock.Object, unitOfWorkMock.Object);
            var transactionId = Guid.NewGuid();
            var command = new CompleteTransactionCommand(transactionId);

            transactionRepositoryMock.Setup(x => x.GetByIdAsync(transactionId, It.IsAny<bool>()))
                .ReturnsAsync((Transaction?)null);

            //Act
            var result = await handler.Handle(command, CancellationToken.None);

            //Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Transaction.NotFound", result.Error.Code);
            Assert.Equal("The transaction was not found", result.Error.Description);
            unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_TransactionFound_CompletesTransactionAndReturnsSuccess()
        {
            //Arrange
            var transactionRepositoryMock = new Mock<ITransactionRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var handler = new CompleteTransactionHandler(transactionRepositoryMock.Object, unitOfWorkMock.Object);

            var factory = new TransactionFactory();
            var transaction = factory.CreateEntryTransaction(
                Guid.NewGuid(),
                new List<(Guid itemId, int quantity, decimal unitaryCost)> { (Guid.NewGuid(), 10, 10.0m) });
            var command = new CompleteTransactionCommand(transaction.Id);

            transactionRepositoryMock.Setup(x => x.GetByIdAsync(transaction.Id, It.IsAny<bool>()))
                .ReturnsAsync(transaction);

            //Act
            var result = await handler.Handle(command, CancellationToken.None);

            //Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Value);
            Assert.Equal(TransactionStatus.Completed, transaction.Status);
            unitOfWorkMock.Verify(x => x.CommitAsync(CancellationToken.None), Times.Once);
        }
    }
}
