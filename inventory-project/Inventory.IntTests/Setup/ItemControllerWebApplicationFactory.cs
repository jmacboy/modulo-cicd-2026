using Inventory.Domain.Items;
using Inventory.Infrastructure.Persistence.StoredModel;
using Inventory.WebApi.Controllers;
using Joseco.DDD.Core.Abstractions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventory.IntTests.Setup
{
    public class ItemControllerWebApplicationFactory : WebApplicationFactory<ItemController>
    {
        private readonly string _dbName = $"IntTests_Item_{Guid.NewGuid()}";

        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IItemRepository));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                var unitDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IUnitOfWork));
                if (unitDescriptor != null)
                {
                    services.Remove(unitDescriptor);
                }
                services.AddSingleton<IItemRepository, InMemoryItemRepository>();
                services.AddSingleton<IUnitOfWork, InMemoryUnitOfWork>();

                // Calling AddDbContext<PersistenceDbContext> again would ADD to, not replace, the
                // Npgsql configuration already registered by Program.cs, causing EF Core to see two
                // providers on the same context. Register an already-built DbContextOptions instance
                // instead, bypassing that additive pipeline entirely.
                services.RemoveAll<DbContextOptions<PersistenceDbContext>>();
                services.AddSingleton(new DbContextOptionsBuilder<PersistenceDbContext>()
                    .UseInMemoryDatabase(_dbName)
                    .Options);
            });
        }
    }
}
