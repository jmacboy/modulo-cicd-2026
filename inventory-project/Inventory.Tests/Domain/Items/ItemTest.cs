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
    }
}
