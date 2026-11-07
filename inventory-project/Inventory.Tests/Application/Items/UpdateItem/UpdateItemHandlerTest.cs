using Inventory.Application.Items.UpdateItem;
using Inventory.Domain.Items;
using Joseco.DDD.Core.Abstractions;
using Joseco.DDD.Core.Results;
using Moq;

namespace Inventory.Tests.Application.Items.UpdateItem
{
    public class UpdateItemHandlerTest
    {
        private static Item CreateDefaultItem() => new Item(Guid.NewGuid(), "Widget");

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
            var handler = new UpdateItemHandler(itemRepositoryMock.Object, unitOfWorkMock.Object);
            var command = new UpdateItemCommand(item.Id, "Gadget");

            //Act
            var result = await handler.Handle(command, CancellationToken.None);

            //Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Value);
            itemRepositoryMock.Verify(x => x.UpdateAsync(It.Is<Item>(i => i.Id == item.Id && i.Name == "Gadget")), Times.Once);
            unitOfWorkMock.Verify(x => x.CommitAsync(CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task Handle_ItemNotFound_ReturnsNotFoundFailure()
        {
            //Arrange
            var itemRepositoryMock = new Mock<IItemRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            itemRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync((Item?)null);
            var handler = new UpdateItemHandler(itemRepositoryMock.Object, unitOfWorkMock.Object);
            var command = new UpdateItemCommand(Guid.NewGuid(), "Gadget");

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
        public async Task Handle_InvalidName_ReturnsFailureWithoutCommit()
        {
            //Arrange
            var item = CreateDefaultItem();
            var itemRepositoryMock = new Mock<IItemRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            itemRepositoryMock
                .Setup(x => x.GetByIdAsync(item.Id, It.IsAny<bool>()))
                .ReturnsAsync(item);
            var handler = new UpdateItemHandler(itemRepositoryMock.Object, unitOfWorkMock.Object);
            var command = new UpdateItemCommand(item.Id, "   ");

            //Act
            var result = await handler.Handle(command, CancellationToken.None);

            //Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ItemErrors.NameIsRequired().Code, result.Error.Code);
            itemRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Item>()), Times.Never);
            unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
