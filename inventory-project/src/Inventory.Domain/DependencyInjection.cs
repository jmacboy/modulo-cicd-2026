using Inventory.Domain.Items;
using Inventory.Domain.Transactions;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.Domain;

public static class DependencyInjection
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        //Scoped, Transient, Singleton
        services.AddSingleton<ICostStrategy, AverageCostStrategy>();
        services.AddSingleton<ITransactionFactory, TransactionFactory>();

        return services;
    }
}
