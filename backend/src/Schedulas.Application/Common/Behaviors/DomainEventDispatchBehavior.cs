using MediatR;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Domain.Common;

namespace Schedulas.Application.Common.Behaviors;

/// <summary>
/// Wraps a Domain event so it can travel through MediatR's INotification
/// pipeline to Application-layer handlers, without Domain itself knowing
/// MediatR exists (Architecture §3.1 — Domain raises events, Application
/// reacts).
/// </summary>
public sealed class DomainEventNotification<TDomainEvent> : INotification
    where TDomainEvent : DomainEvent
{
    public TDomainEvent DomainEvent { get; }
    public DomainEventNotification(TDomainEvent domainEvent) => DomainEvent = domainEvent;
}

/// <summary>
/// After the wrapped handler completes (which includes any SaveChangesAsync
/// calls), collects whatever domain events entities buffered during this
/// request and publishes each as a DomainEventNotification&lt;T&gt;. Placed
/// innermost in the pipeline (closest to the handler) so events are only
/// dispatched once the unit of work has actually committed.
/// </summary>
public sealed class DomainEventDispatchBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IApplicationDbContextEventSource _eventSource;
    private readonly IPublisher _publisher;

    public DomainEventDispatchBehavior(IApplicationDbContextEventSource eventSource, IPublisher publisher)
    {
        _eventSource = eventSource;
        _publisher = publisher;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();

        var events = _eventSource.CollectAndClearDomainEvents();
        foreach (var domainEvent in events)
        {
            var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
            var notification = (INotification)Activator.CreateInstance(notificationType, domainEvent)!;
            await _publisher.Publish(notification, cancellationToken);
        }

        return response;
    }
}

/// <summary>
/// Minimal surface the dispatch behavior needs from the DbContext, kept as
/// its own interface (rather than piling onto IApplicationDbContext) since
/// it's a cross-cutting mechanism, not a data-access concern.
/// Implemented by SchedulasDbContext.CollectAndClearDomainEvents().
/// </summary>
public interface IApplicationDbContextEventSource
{
    IReadOnlyList<DomainEvent> CollectAndClearDomainEvents();
}
