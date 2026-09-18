using AppointmentScheduler.Domain.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AppointmentScheduler.Infrastructure.Events;

/// <summary>
/// A minimal in-process domain event dispatcher: no message bus, no
/// serialization, just "resolve whatever IDomainEventHandler&lt;T&gt;
/// implementations are registered for this event's runtime type and invoke
/// them". Good enough for a single-process app; a real distributed system
/// would swap this out for an outbox + message bus without the domain or
/// application layers needing to change.
/// </summary>
public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(IServiceProvider serviceProvider, ILogger<DomainEventDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task DispatchAndClearEventsAsync(
        IEnumerable<AggregateRoot> aggregatesWithEvents,
        CancellationToken cancellationToken = default)
    {
        var aggregates = aggregatesWithEvents.ToList();

        var events = aggregates
            .SelectMany(aggregate => aggregate.DomainEvents)
            .ToList();

        foreach (var aggregate in aggregates)
        {
            aggregate.ClearDomainEvents();
        }

        foreach (var domainEvent in events)
        {
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
            var handlers = _serviceProvider.GetServices(handlerType);

            foreach (var handler in handlers)
            {
                if (handler is null)
                    continue;

                var method = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync))!;
                var task = (Task)method.Invoke(handler, [domainEvent, cancellationToken])!;
                await task;
            }

            _logger.LogDebug("Dispatched domain event {EventType}", domainEvent.GetType().Name);
        }
    }
}
