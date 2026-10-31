using Inventory.Application.Items.EventHandlers;
using Inventory.Domain.Items;
using Inventory.Domain.Transactions;
using Inventory.Domain.Transactions.Events;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Inventory.Tests.Application.Items.EventHandlers
{
    public class UpdateStockWhenTransactionCompletedTest
    {
        private static Item CreateDefaultItem(Guid id) => new Item(id, "Widget");

        private static Mock<ICostStrategy> CreatePassthroughCostStrategyMock()
        {
            var costStrategyMock = new Mock<ICostStrategy>();
            costStrategyMock
                .Setup(x => x.CalculateNewCost(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<decimal>()))
                .Returns<int, decimal, int, decimal>((currentStock, currentCost, newStock, newCost) => newCost);
            return costStrategyMock;
        }

        [Fact]
        public async Task Handle_EntryTransaction_AddsStockAndUpdatesRepository()
        {
            //Arrange
            var itemId = Guid.NewGuid();
            var item = CreateDefaultItem(itemId);
            var itemRepositoryMock = new Mock<IItemRepository>();
            itemRepositoryMock.Setup(x => x.GetByIdAsync(itemId, It.IsAny<bool>())).ReturnsAsync(item);
            var costStrategyMock = CreatePassthroughCostStrategyMock();
            var handler = new UpdateStockWhenTransactionCompleted(itemRepositoryMock.Object, costStrategyMock.Object);
            var details = new List<TransactionCompleted.TransactionCompletedDetail>
            {
                new(itemId, 20, 15.50m)
            };
            var domainEvent = new TransactionCompleted(Guid.NewGuid(), TransactionType.Entry, details);

            //Act
            await handler.Handle(domainEvent, CancellationToken.None);

            //Assert
            Assert.Equal(20, item.Stock.Value);
            Assert.Equal(15.50m, item.UnitaryCost.Value);
            itemRepositoryMock.Verify(x => x.UpdateAsync(It.Is<Item>(i => i.Id == itemId)), Times.Once);
        }

        [Fact]
        public async Task Handle_ExitTransaction_AppliesReservationAndUpdatesRepository()
        {
            //Arrange
            var itemId = Guid.NewGuid();
            var item = CreateDefaultItem(itemId);
            var costStrategyMock = CreatePassthroughCostStrategyMock();
            item.AddStock(20, 10.0m, costStrategyMock.Object);
            item.ReserveStock(5);
            var itemRepositoryMock = new Mock<IItemRepository>();
            itemRepositoryMock.Setup(x => x.GetByIdAsync(itemId, It.IsAny<bool>())).ReturnsAsync(item);
            var handler = new UpdateStockWhenTransactionCompleted(itemRepositoryMock.Object, costStrategyMock.Object);
            var details = new List<TransactionCompleted.TransactionCompletedDetail>
            {
                new(itemId, 5, 0m)
            };
            var domainEvent = new TransactionCompleted(Guid.NewGuid(), TransactionType.Exit, details);

            //Act
            await handler.Handle(domainEvent, CancellationToken.None);

            //Assert
            Assert.Equal(15, item.Stock.Value);
            Assert.Equal(0, item.Reserved.Value);
            itemRepositoryMock.Verify(x => x.UpdateAsync(It.Is<Item>(i => i.Id == itemId)), Times.Once);
        }

        [Theory]
        [InlineData(TransactionType.Entry)]
        [InlineData(TransactionType.Exit)]
        public async Task Handle_ItemNotFound_SkipsItemWithoutUpdatingRepository(TransactionType transactionType)
        {
            //Arrange
            var itemId = Guid.NewGuid();
            var itemRepositoryMock = new Mock<IItemRepository>();
            itemRepositoryMock.Setup(x => x.GetByIdAsync(itemId, It.IsAny<bool>())).ReturnsAsync((Item?)null);
            var costStrategyMock = CreatePassthroughCostStrategyMock();
            var handler = new UpdateStockWhenTransactionCompleted(itemRepositoryMock.Object, costStrategyMock.Object);
            var details = new List<TransactionCompleted.TransactionCompletedDetail>
            {
                new(itemId, 10, 5.0m)
            };
            var domainEvent = new TransactionCompleted(Guid.NewGuid(), transactionType, details);

            //Act
            await handler.Handle(domainEvent, CancellationToken.None);

            //Assert
            itemRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Item>()), Times.Never);
        }

        [Fact]
        public async Task Handle_MultipleDetailsWithSomeItemsMissing_UpdatesOnlyExistingItems()
        {
            //Arrange
            var foundItemId = Guid.NewGuid();
            var missingItemId = Guid.NewGuid();
            var foundItem = CreateDefaultItem(foundItemId);
            var itemRepositoryMock = new Mock<IItemRepository>();
            itemRepositoryMock.Setup(x => x.GetByIdAsync(foundItemId, It.IsAny<bool>())).ReturnsAsync(foundItem);
            itemRepositoryMock.Setup(x => x.GetByIdAsync(missingItemId, It.IsAny<bool>())).ReturnsAsync((Item?)null);
            var costStrategyMock = CreatePassthroughCostStrategyMock();
            var handler = new UpdateStockWhenTransactionCompleted(itemRepositoryMock.Object, costStrategyMock.Object);
            var details = new List<TransactionCompleted.TransactionCompletedDetail>
            {
                new(foundItemId, 10, 5.0m),
                new(missingItemId, 10, 5.0m)
            };
            var domainEvent = new TransactionCompleted(Guid.NewGuid(), TransactionType.Entry, details);

            //Act
            await handler.Handle(domainEvent, CancellationToken.None);

            //Assert
            Assert.Equal(10, foundItem.Stock.Value);
            itemRepositoryMock.Verify(x => x.UpdateAsync(It.Is<Item>(i => i.Id == foundItemId)), Times.Once);
            itemRepositoryMock.Verify(x => x.UpdateAsync(It.Is<Item>(i => i.Id == missingItemId)), Times.Never);
        }
    }
}
