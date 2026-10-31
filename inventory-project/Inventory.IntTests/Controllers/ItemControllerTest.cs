using Inventory.Domain.Items;
using Inventory.Infrastructure.Persistence.PersistenceModel.Entities;
using Inventory.Infrastructure.Persistence.StoredModel;
using Inventory.IntTests.Setup;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Inventory.IntTests.Controllers
{
    public class ItemControllerTest : IClassFixture<ItemControllerWebApplicationFactory>
    {
        private readonly ItemControllerWebApplicationFactory _factory;
        public ItemControllerTest(ItemControllerWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task CreateItem_WithValidRequest()
        {
            //Arrange
            var client = _factory.CreateClient();
            var itemGuid = Guid.NewGuid();
            var itemName = "Test Item";
            //Act
            var response = await client.PostAsJsonAsync("/api/Item", new { Id = itemGuid, ItemName = itemName });

            //Assert
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadFromJsonAsync<ItemCreateResponse>();
            Assert.NotNull(body);
            Assert.True(body.IsSuccess);
            Assert.False(body.IsFailure);
            Assert.Equal(itemGuid, body.Value);

            using var scope = _factory.Services.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IItemRepository>();
            var item = await repository.GetByIdAsync(itemGuid);
            Assert.NotNull(item);
            Assert.Equal(itemName, item.Name);
        }

        [Fact]
        public async Task CreateItem_WithDuplicateRequest()
        {
            //Arrange
            var client = _factory.CreateClient();
            var itemGuid = Guid.NewGuid();
            var itemName = "Test Item";
            var itemName2 = "Name";

            //Act
            var response = await client.PostAsJsonAsync("/api/Item", new { Id = itemGuid, ItemName = itemName });
            var response2 = await client.PostAsJsonAsync("/api/Item", new { Id = itemGuid, ItemName = itemName2 });

            //Assert
            response.EnsureSuccessStatusCode();
            var statusCode = response2.StatusCode;
            Assert.Equal(System.Net.HttpStatusCode.InternalServerError, statusCode);

            using var scope = _factory.Services.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IItemRepository>();
            var item = await repository.GetByIdAsync(itemGuid);
            Assert.NotNull(item);
            Assert.Equal(itemName, item.Name);
        }

        [Fact]
        public async Task GetItems_WithSeededItem_ReturnsIt()
        {
            //Arrange
            var client = _factory.CreateClient();
            var itemGuid = Guid.NewGuid();
            var itemName = "Test Item";

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PersistenceDbContext>();
                dbContext.Item.Add(new ItemPersistenceModel
                {
                    Id = itemGuid,
                    ItemName = itemName,
                    Stock = 10,
                    Reserverd = 0,
                    Available = 10,
                    UnitaryCost = 5.0m
                });
                await dbContext.SaveChangesAsync();
            }

            //Act
            var response = await client.GetAsync("/api/Item");

            //Assert
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadFromJsonAsync<GetItemsResponse>();
            Assert.NotNull(body);
            Assert.Contains(body.Value, i => i.Id == itemGuid && i.ItemName == itemName);
        }
    }
}
