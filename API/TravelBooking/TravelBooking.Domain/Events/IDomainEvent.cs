using System;


namespace TravelBooking.Domain.Events;

public interface IDomainEvent 


{
    DateTime DateOccurred { get; }                //---Olayin gerceklestigi tarihi ve saati kaydetmek icin kullanilir.


}

