using Inventory.Application.Items.DeleteItem;
using Inventory.Domain.Items;
using Joseco.DDD.Core.Abstractions;
using Joseco.DDD.Core.Results;
using Moq;

namespace Inventory.Tests.Application.Items.DeleteItem
{
    public class DeleteItemHandlerTest
    {
        private static Item CreateDefaultItem() => new Item(Guid.NewGuid(), "Widget");

        [Fact]
        public async Task Handle_ItemNotFound_ReturnsNotFoundFailure()
        {
            //Arrange
            var itemRepositoryMock = new Mock<IItemRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            itemRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync((Item?)null);
            var handler = new DeleteItemHandler(itemRepositoryMock.Object, unitOfWorkMock.Object);
            var command = new DeleteItemCommand(Guid.NewGuid());

            //Act
            var result = await handler.Handle(command, CancellationToken.None);

            //Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.Error.Type);
            Assert.Equal("ItemNotFound", result.Error.Code);
            itemRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Item>()), Times.Never);
            unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ExistingItem_ReturnsSuccessAndCommits()
        {
            //Arrange
            var item = CreateDefaultItem();
            var itemRepositoryMock = new Mock<IItemRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            itemRepositoryMock
                .Setup(x => x.GetByIdAsync(item.Id, It.IsAny<bool>()))
                .ReturnsAsync(item);
            var handler = new DeleteItemHandler(itemRepositoryMock.Object, unitOfWorkMock.Object);
            var command = new DeleteItemCommand(item.Id);

            //Act
            var result = await handler.Handle(command, CancellationToken.None);

            //Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Value);
            itemRepositoryMock.Verify(x => x.UpdateAsync(It.Is<Item>(i => i.Id == item.Id && i.IsDeleted)), Times.Once);
            unitOfWorkMock.Verify(x => x.CommitAsync(CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task Handle_ItemWithReservedStock_ReturnsFailureWithoutCommit()
        {
            //Arrange
            var item = CreateDefaultItem();
            item.AddStock(10, 5.0m, new AverageCostStrategy());
            item.ReserveStock(4);
            var itemRepositoryMock = new Mock<IItemRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            itemRepositoryMock
                .Setup(x => x.GetByIdAsync(item.Id, It.IsAny<bool>()))
                .ReturnsAsync(item);
            var handler = new DeleteItemHandler(itemRepositoryMock.Object, unitOfWorkMock.Object);
            var command = new DeleteItemCommand(item.Id);

            //Act
            var result = await handler.Handle(command, CancellationToken.None);

            //Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ItemErrors.CannotDeleteWithReservedStock(4).Code, result.Error.Code);
            itemRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Item>()), Times.Never);
            unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
