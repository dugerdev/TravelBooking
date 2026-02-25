namespace TravelBooking.Domain.Events;

//---Domain event handler interface'i---//
public interface IDomainEventHandler<in T> where T : IDomainEvent
{
    Task Handle(T domainEvent, CancellationToken cancellationToken = default);
}
