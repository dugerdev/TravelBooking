using TravelBooking.Domain.Events;
using System.Collections.Generic;

namespace TravelBooking.Domain.Common;

//---Domain Event'leri yoneten interface---//
public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }                    //---Entity'nin sahip oldugu domain event'lerin listesi---//

    void AddDomainEvent(IDomainEvent domainEvent);                             //---Domain event ekleyen metot---//

    void ClearDomainEvents();                                                  //---Domain event'leri temizleyen metot---//
}
