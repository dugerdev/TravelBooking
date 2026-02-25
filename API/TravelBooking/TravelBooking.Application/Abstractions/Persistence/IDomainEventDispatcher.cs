using TravelBooking.Domain.Events;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TravelBooking.Application.Abstractions.Persistence;

//---Domain Event'leri dispatch eden interface---//
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Domain event'leri dispatch eder (handler'lari cagirir)
    /// </summary>
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}

