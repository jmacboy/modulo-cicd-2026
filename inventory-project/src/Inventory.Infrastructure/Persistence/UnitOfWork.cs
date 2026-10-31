using Inventory.Infrastructure.Persistence.DomainModel;
using Joseco.DDD.Core.Abstractions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventory.Infrastructure.Persistence;

internal class UnitOfWork : IUnitOfWork
{
    private readonly DomainDbContext _dbContext;
    private readonly IMediator _mediator;

    public UnitOfWork(DomainDbContext dbContext, IMediator mediator)
    {
        _dbContext = dbContext;
        _mediator = mediator;
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {

        //Get domain events
        var domainEvents = _dbContext.ChangeTracker
            .Entries<Entity>()
            .Where(x => x.Entity.DomainEvents.Any())
            .Select(entityEntry =>
            {
                var domainEvents = entityEntry.Entity
                                .DomainEvents
                                .ToImmutableArray();
                entityEntry.Entity.ClearDomainEvents();

                return domainEvents;
            })
            .SelectMany(domainEvents => domainEvents)
            .ToList();

        //[[e1, e2], [e3]] => [e1, e2, e3]

        //Publish Domain Events
        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }


        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
