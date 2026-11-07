using Inventory.Domain.Items;
using Joseco.DDD.Core.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventory.Tests.Domain.Items
{
    public class ItemTest
    {
        [Fact]
        public void Item_DefaultValues()
        {
            //Arrange
            var item = new Item(Guid.NewGuid(), "Test Item");

            //Assert
            Assert.NotNull(item);
            Assert.Equal("Test Item", item.Name);
            Assert.Equal(0, item.Stock.Value);
            Assert.Equal(0, item.Reserved.Value);
            Assert.Equal(0, item.Available.Value);
            Assert.Equal(0.0m, item.UnitaryCost.Value);
        }
        [Fact]
        public void Item_NullName()
        {
            Assert.Throws<DomainException>(() => new Item(Guid.NewGuid(), null));

            try
            {
                new Item(Guid.NewGuid(), null);
            }
            catch (DomainException e)
            {
                Assert.Equal(ItemErrors.NameIsRequired().Description, e.Error.Description);
            }
        }
        [Fact]
        public void AddStock_SingleItem()
        {
            //Arrange
            var item = new Item(Guid.NewGuid(), "Test Item");
            var strategy = new AverageCostStrategy();

            //Act
            item.AddStock(10, 10.0m, strategy);

            //Assert
            Assert.Equal(10, item.Stock.Value);
            Assert.Equal(10.0m, item.UnitaryCost.Value);
            Assert.Equal(10, item.Available.Value);
        }

        [Fact]
        public void AddStock_AverageCost()
        {
            //Arrange
            var item = new Item(Guid.NewGuid(), "Test Item");
            var strategy = new AverageCostStrategy();

            //Act
            item.AddStock(10, 10.0m, strategy);
            item.AddStock(5, 10.0m, strategy);

            //Assert
            Assert.Equal(15, item.Stock.Value);
            Assert.Equal(10.0m, item.UnitaryCost.Value);
            Assert.Equal(15, item.Available.Value);
        }

        [Fact]
        public void AddStock_NegativeQuantity()
        {
            //Arrange
            var item = new Item(Guid.NewGuid(), "Test Item");
            var strategy = new AverageCostStrategy();

            //Act + assert
            Assert.Throws<DomainException>(() => item.AddStock(-5, 10.0m, strategy));

            try
            {
                item.AddStock(-5, 10.0m, strategy);
            }
            catch (DomainException e)
            {
                Assert.Equal(ItemErrors.NonNegativeStock().Description, e.Error.Description);
            }
        }
        [Fact]
        public void AddStock_StrategyNull()
        {
            //Arrange
            var item = new Item(Guid.NewGuid(), "Test Item");
            ICostStrategy? strategy = null;

            //Act + assert
            Assert.Throws<DomainException>(() => item.AddStock(5, 10.0m, strategy));

            try
            {
                item.AddStock(5, 10.0m, strategy);
            }
            catch (DomainException e)
            {
                Assert.Equal(ItemErrors.CostStrategyNotProvided().Description, e.Error.Description);
            }
        }

        [Fact]
        public void Delete_ActiveItem_MarksItemAsDeleted()
        {
            //Arrange
            var item = new Item(Guid.NewGuid(), "Widget");

            //Act
            item.Delete();

            //Assert
            Assert.True(item.IsDeleted);
        }

        [Fact]
        public void Delete_ActiveItem_SetsDeletedAtToCurrentUtcTime()
        {
            //Arrange
            var item = new Item(Guid.NewGuid(), "Widget");
            var before = DateTime.UtcNow;

            //Act
            item.Delete();

            //Assert
            var after = DateTime.UtcNow;
            Assert.NotNull(item.DeletedAt);
            Assert.InRange(item.DeletedAt!.Value, before, after);
        }

        [Fact]
        public void Delete_AlreadyDeletedItem_ThrowsDomainException()
        {
            //Arrange
            var item = new Item(Guid.NewGuid(), "Widget");
            item.Delete();

            //Act
            var ex = Assert.Throws<DomainException>(() => item.Delete());

            //Assert
            Assert.Equal(ItemErrors.ItemAlreadyDeleted().Code, ex.Error.Code);
            Assert.Equal(ItemErrors.ItemAlreadyDeleted().Description, ex.Error.Description);
        }

        [Fact]
        public void Delete_ItemWithReservedStock_ThrowsDomainException()
        {
            //Arrange
            var item = new Item(Guid.NewGuid(), "Widget");
            item.AddStock(10, 5.0m, new AverageCostStrategy());
            item.ReserveStock(3);

            //Act
            var ex = Assert.Throws<DomainException>(() => item.Delete());

            //Assert
            Assert.Equal(ItemErrors.CannotDeleteWithReservedStock(3).Code, ex.Error.Code);
            Assert.Equal(ItemErrors.CannotDeleteWithReservedStock(3).Description, ex.Error.Description);
            Assert.False(item.IsDeleted);
            Assert.Null(item.DeletedAt);
        }

        [Fact]
        public void Rename_ValidName_UpdatesName()
        {
            //Arrange
            var item = new Item(Guid.NewGuid(), "Widget");

            //Act
            item.Rename("Gadget");

            //Assert
            Assert.Equal("Gadget", item.Name);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Rename_NullOrWhitespaceName_ThrowsDomainException(string? newName)
        {
            //Arrange
            var item = new Item(Guid.NewGuid(), "Widget");

            //Act
            var ex = Assert.Throws<DomainException>(() => item.Rename(newName!));

            //Assert
            Assert.Equal(ItemErrors.NameIsRequired().Code, ex.Error.Code);
            Assert.Equal(ItemErrors.NameIsRequired().Description, ex.Error.Description);
            Assert.Equal("Widget", item.Name);
        }

        [Fact]
        public void Rename_DeletedItem_ThrowsDomainException()
        {
            //Arrange
            var item = new Item(Guid.NewGuid(), "Widget");
            item.Delete();

            //Act
            var ex = Assert.Throws<DomainException>(() => item.Rename("Gadget"));

            //Assert
            Assert.Equal(ItemErrors.CannotUpdateDeletedItem().Code, ex.Error.Code);
            Assert.Equal(ItemErrors.CannotUpdateDeletedItem().Description, ex.Error.Description);
            Assert.Equal("Widget", item.Name);
        }
    }
}
