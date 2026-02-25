using TravelBooking.Application.Abstractions.Persistence;
using TravelBooking.Domain.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TravelBooking.Infrastructure.Repositories;

//---Domain Event'leri dispatch eden implementation---//
//---Handler'lari DI container'dan alarak cagirir---//
public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(
        IServiceProvider serviceProvider,
        ILogger<DomainEventDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            try
            {
                _logger.LogInformation(
                    "Dispatching domain event: {EventType} at {DateOccurred}",
                    domainEvent.GetType().Name,
                    domainEvent.DateOccurred);

                //---Handler tipini bul---//
                var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
                
                //---Tum handler'lari DI container'dan al---//
                var handlers = _serviceProvider.GetServices(handlerType);
                
                if (handlers != null && handlers.Any())
                {
                    foreach (var handler in handlers)
                    {
                        var handleMethod = handlerType.GetMethod("Handle");
                        if (handleMethod != null)
                        {
                            var task = (Task)handleMethod.Invoke(handler, new object[] { domainEvent, cancellationToken })!;
                            await task;
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("No handler found for domain event: {EventType}", domainEvent.GetType().Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error dispatching domain event: {EventType}",
                    domainEvent.GetType().Name);
                
                //---Event dispatch hatasi, islemi durdurmayabilir veya durdurabilir---//
                //---Domain event dispatch hatasi kritik ise exception firlatilabilir---//
                // throw;
            }
        }
    }
}

