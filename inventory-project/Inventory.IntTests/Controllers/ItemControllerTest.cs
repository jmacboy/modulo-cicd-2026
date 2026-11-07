using Inventory.Domain.Items;
using Inventory.Domain.Transactions;
using Inventory.Infrastructure.Persistence.PersistenceModel.Entities;
using Inventory.Infrastructure.Persistence.StoredModel;
using Inventory.IntTests.Setup;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        [Fact]
        public async Task UpdateItem_WithValidRequest()
        {
            //Arrange
            var client = _factory.CreateClient();
            var itemGuid = Guid.NewGuid();
            var newName = "Gadget";
            var createResponse = await client.PostAsJsonAsync("/api/Item", new { Id = itemGuid, ItemName = "Widget" });
            createResponse.EnsureSuccessStatusCode();

            //Act
            var response = await client.PutAsJsonAsync($"/api/Item/{itemGuid}", new { Name = newName });

            //Assert
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadFromJsonAsync<ItemUpdateResponse>();
            Assert.NotNull(body);
            Assert.True(body.IsSuccess);
            Assert.False(body.IsFailure);
            Assert.True(body.Value);

            using var scope = _factory.Services.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IItemRepository>();
            var item = await repository.GetByIdAsync(itemGuid);
            Assert.NotNull(item);
            Assert.Equal(newName, item.Name);
        }

        [Fact]
        public async Task UpdateItem_WithUnknownId()
        {
            //Arrange
            var client = _factory.CreateClient();
            var itemGuid = Guid.NewGuid();

            //Act
            var response = await client.PutAsJsonAsync($"/api/Item/{itemGuid}", new { Name = "Gadget" });

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateItem_WithEmptyName()
        {
            //Arrange
            var client = _factory.CreateClient();
            var itemGuid = Guid.NewGuid();
            var itemName = "Widget";
            var createResponse = await client.PostAsJsonAsync("/api/Item", new { Id = itemGuid, ItemName = itemName });
            createResponse.EnsureSuccessStatusCode();

            //Act
            var response = await client.PutAsJsonAsync($"/api/Item/{itemGuid}", new { Name = "" });

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            Assert.NotNull(body);
            Assert.Equal(ItemErrors.NameIsRequired().Code, body.Code);

            using var scope = _factory.Services.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IItemRepository>();
            var item = await repository.GetByIdAsync(itemGuid);
            Assert.NotNull(item);
            Assert.Equal(itemName, item.Name);
        }

        [Fact]
        public async Task GetItems_WithSeededItem_ReturnsUnitaryCost()
        {
            //Arrange
            var client = _factory.CreateClient();
            var itemGuid = Guid.NewGuid();

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PersistenceDbContext>();
                dbContext.Item.Add(new ItemPersistenceModel
                {
                    Id = itemGuid,
                    ItemName = "Widget",
                    Stock = 10,
                    Reserverd = 0,
                    Available = 10,
                    UnitaryCost = 7.25m
                });
                await dbContext.SaveChangesAsync();
            }

            //Act
            var response = await client.GetAsync("/api/Item");

            //Assert
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadFromJsonAsync<GetItemsResponse>();
            Assert.NotNull(body);
            var item = Assert.Single(body.Value, i => i.Id == itemGuid);
            Assert.Equal(7.25m, item.UnitaryCost);
        }

        [Fact]
        public async Task AddStock_WithValidRequest()
        {
            //Arrange
            var client = _factory.CreateClient();
            var itemGuid = Guid.NewGuid();
            var userGuid = Guid.NewGuid();
            using (var scope = _factory.Services.CreateScope())
            {
                var itemRepository = scope.ServiceProvider.GetRequiredService<IItemRepository>();
                await itemRepository.AddAsync(new Item(itemGuid, "Widget"));
            }

            //Act
            var response = await client.PostAsJsonAsync($"/api/Item/{itemGuid}/stock",
                new { UserCreatorId = userGuid, Quantity = 10, UnitaryCost = 5.5m });

            //Assert
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadFromJsonAsync<AddStockResponse>();
            Assert.NotNull(body);
            Assert.True(body.IsSuccess);
            Assert.NotEqual(Guid.Empty, body.Value);

            using var assertScope = _factory.Services.CreateScope();
            var transactionRepository = assertScope.ServiceProvider.GetRequiredService<ITransactionRepository>();
            var transaction = await transactionRepository.GetByIdAsync(body.Value);
            Assert.NotNull(transaction);
            Assert.Equal(TransactionType.Entry, transaction.Type);
            Assert.Equal(TransactionStatus.Completed, transaction.Status);
            Assert.Contains(transaction.Items, i => i.ItemId == itemGuid && i.Quantity == 10);
        }

        [Fact]
        public async Task AddStock_WithUnknownItem()
        {
            //Arrange
            var client = _factory.CreateClient();
            var itemGuid = Guid.NewGuid();

            //Act
            var response = await client.PostAsJsonAsync($"/api/Item/{itemGuid}/stock",
                new { UserCreatorId = Guid.NewGuid(), Quantity = 10, UnitaryCost = 5.5m });

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AddStock_WithZeroQuantity()
        {
            //Arrange
            var client = _factory.CreateClient();
            var itemGuid = Guid.NewGuid();
            using (var scope = _factory.Services.CreateScope())
            {
                var itemRepository = scope.ServiceProvider.GetRequiredService<IItemRepository>();
                await itemRepository.AddAsync(new Item(itemGuid, "Widget"));
            }

            //Act
            var response = await client.PostAsJsonAsync($"/api/Item/{itemGuid}/stock",
                new { UserCreatorId = Guid.NewGuid(), Quantity = 0, UnitaryCost = 5.5m });

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
