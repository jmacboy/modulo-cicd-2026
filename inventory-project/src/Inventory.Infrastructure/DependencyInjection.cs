using Inventory.Application;
using Inventory.Domain.Items;
using Inventory.Domain.Transactions;
using Inventory.Domain.Users;
using Inventory.Infrastructure.Persistence;
using Inventory.Infrastructure.Persistence.DomainModel;
using Inventory.Infrastructure.Persistence.Respositories;
using Inventory.Infrastructure.Persistence.StoredModel;
using Joseco.DDD.Core.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Inventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddApplication()
            .AddPersistence(configuration);

        services.AddMediatR(config =>
               config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly())
           );

        return services;
    }

    private static void AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var dbConnectionString = configuration.GetConnectionString("InventoryDatabase");

        services.AddDbContext<PersistenceDbContext>(context =>
                context.UseNpgsql(dbConnectionString));
        services.AddDbContext<DomainDbContext>(context =>
                context.UseNpgsql(dbConnectionString));

        services.AddScoped<IDatabase, PersistenceDbContext>();
        //Singleton, Scoped, Transient


        // Register repositories
        services.AddScoped<IDatabase, PersistenceDbContext>();
        services.AddScoped<IItemRepository, ItemRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }
}
