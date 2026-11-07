using Inventory.Application.Items.AddStock;
using Inventory.Domain.Items;
using Inventory.Domain.Transactions;
using Joseco.DDD.Core.Abstractions;
using Joseco.DDD.Core.Results;
using Moq;

namespace Inventory.Tests.Application.Items.AddStock
{
    public class AddStockHandlerTest
    {
        private readonly Mock<IItemRepository> _itemRepositoryMock = new();
        private readonly Mock<ITransactionFactory> _transactionFactoryMock = new();
        private readonly Mock<ITransactionRepository> _transactionRepositoryMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

        private static Item CreateDefaultItem() => new Item(Guid.NewGuid(), "Widget");

        private static Transaction CreateEntryTransaction(Guid userId, Guid itemId, int quantity, decimal cost) =>
            new TransactionFactory().CreateEntryTransaction(userId, [(itemId, quantity, cost)]);

        private AddStockHandler CreateHandler() => new AddStockHandler(
            _itemRepositoryMock.Object,
            _transactionFactoryMock.Object,
            _transactionRepositoryMock.Object,
            _unitOfWorkMock.Object);

        private Transaction ArrangeExistingItem(AddStockCommand command)
        {
            var transaction = CreateEntryTransaction(command.UserCreatorId, command.ItemId, command.Quantity, command.UnitaryCost);
            _itemRepositoryMock
                .Setup(x => x.GetByIdAsync(command.ItemId, It.IsAny<bool>()))
                .ReturnsAsync(new Item(command.ItemId, "Widget"));
            _transactionFactoryMock
                .Setup(x => x.CreateEntryTransaction(
                    It.IsAny<Guid>(),
                    It.IsAny<List<(Guid itemId, int quantity, decimal unitaryCost)>>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<string?>()))
                .Returns(transaction);
            return transaction;
        }

        private void VerifyNothingPersisted()
        {
            _transactionFactoryMock.Verify(x => x.CreateEntryTransaction(
                It.IsAny<Guid>(),
                It.IsAny<List<(Guid itemId, int quantity, decimal unitaryCost)>>(),
                It.IsAny<Guid?>(),
                It.IsAny<string?>()), Times.Never);
            _transactionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Transaction>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccessWithTransactionIdAndCommitsOnce()
        {
            //Arrange
            var command = new AddStockCommand(Guid.NewGuid(), Guid.NewGuid(), 10, 5.5m);
            var transaction = ArrangeExistingItem(command);
            var handler = CreateHandler();

            //Act
            var result = await handler.Handle(command, CancellationToken.None);

            //Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(transaction.Id, result.Value);
            _unitOfWorkMock.Verify(x => x.CommitAsync(CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_CreatesEntryTransactionForItem()
        {
            //Arrange
            var command = new AddStockCommand(Guid.NewGuid(), Guid.NewGuid(), 10, 5.5m);
            ArrangeExistingItem(command);
            var handler = CreateHandler();

            //Act
            await handler.Handle(command, CancellationToken.None);

            //Assert
            _transactionFactoryMock.Verify(x => x.CreateEntryTransaction(
                command.UserCreatorId,
                It.Is<List<(Guid itemId, int quantity, decimal unitaryCost)>>(items =>
                    items.Count == 1
                    && items[0].itemId == command.ItemId
                    && items[0].quantity == command.Quantity
                    && items[0].unitaryCost == command.UnitaryCost),
                It.IsAny<Guid?>(),
                It.IsAny<string?>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PersistsCompletedTransaction()
        {
            //Arrange
            var command = new AddStockCommand(Guid.NewGuid(), Guid.NewGuid(), 10, 5.5m);
            var transaction = ArrangeExistingItem(command);
            var handler = CreateHandler();

            //Act
            await handler.Handle(command, CancellationToken.None);

            //Assert
            _transactionRepositoryMock.Verify(x => x.AddAsync(It.Is<Transaction>(t =>
                t.Id == transaction.Id && t.Status == TransactionStatus.Completed)), Times.Once);
        }

        [Fact]
        public async Task Handle_ItemNotFound_ReturnsNotFoundFailureWithoutCommit()
        {
            //Arrange
            _itemRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync((Item?)null);
            var handler = CreateHandler();
            var command = new AddStockCommand(Guid.NewGuid(), Guid.NewGuid(), 10, 5.5m);

            //Act
            var result = await handler.Handle(command, CancellationToken.None);

            //Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.Error.Type);
            Assert.Equal("ItemNotFound", result.Error.Code);
            VerifyNothingPersisted();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public async Task Handle_ZeroOrNegativeQuantity_ReturnsValidationFailureWithoutCommit(int quantity)
        {
            //Arrange
            var item = CreateDefaultItem();
            _itemRepositoryMock
                .Setup(x => x.GetByIdAsync(item.Id, It.IsAny<bool>()))
                .ReturnsAsync(item);
            var handler = CreateHandler();
            var command = new AddStockCommand(item.Id, Guid.NewGuid(), quantity, 5.5m);

            //Act
            var result = await handler.Handle(command, CancellationToken.None);

            //Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ItemErrors.NonNegativeStock().Code, result.Error.Code);
            Assert.Equal(ErrorType.Validation, result.Error.Type);
            VerifyNothingPersisted();
        }

        [Fact]
        public async Task Handle_NegativeUnitaryCost_ReturnsValidationFailureWithoutCommit()
        {
            //Arrange
            var item = CreateDefaultItem();
            _itemRepositoryMock
                .Setup(x => x.GetByIdAsync(item.Id, It.IsAny<bool>()))
                .ReturnsAsync(item);
            var handler = CreateHandler();
            var command = new AddStockCommand(item.Id, Guid.NewGuid(), 10, -0.01m);

            //Act
            var result = await handler.Handle(command, CancellationToken.None);

            //Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ItemErrors.NonNegativeCost().Code, result.Error.Code);
            Assert.Equal(ErrorType.Validation, result.Error.Type);
            VerifyNothingPersisted();
        }

        [Fact]
        public async Task Handle_EmptyUserCreatorId_ReturnsValidationFailureWithoutCommit()
        {
            //Arrange
            var item = CreateDefaultItem();
            _itemRepositoryMock
                .Setup(x => x.GetByIdAsync(item.Id, It.IsAny<bool>()))
                .ReturnsAsync(item);
            var handler = CreateHandler();
            var command = new AddStockCommand(item.Id, Guid.Empty, 10, 5.5m);

            //Act
            var result = await handler.Handle(command, CancellationToken.None);

            //Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(TransactionErrors.CreatorIsRequired().Code, result.Error.Code);
            Assert.Equal(ErrorType.Validation, result.Error.Type);
            VerifyNothingPersisted();
        }
    }
}
