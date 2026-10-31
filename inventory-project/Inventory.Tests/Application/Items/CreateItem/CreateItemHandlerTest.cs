using Inventory.Application.Items.CreateItem;
using Inventory.Domain.Items;
using Joseco.DDD.Core.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventory.Tests.Application.Items.CreateItem
{
    public class CreateItemHandlerTest
    {
        [Fact]
        public async Task HandleTest()
        {
            //Arrange
            var itemRepositoryMock = new Mock<IItemRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var handler = new CreateItemHandler(itemRepositoryMock.Object, unitOfWorkMock.Object);
            var itemId = Guid.NewGuid();
            var command = new CreateItemCommand(itemId, "Test Item");

            //Act
            var result = await handler.Handle(command, CancellationToken.None);

            //Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(itemId, result.Value);

            itemRepositoryMock.Verify(x => x.AddAsync(It.Is<Item>(i => i.Id == command.Id && i.Name == command.ItemName)), Times.Once);
            unitOfWorkMock.Verify(x => x.CommitAsync(CancellationToken.None), Times.Once);
        }
    }
}
