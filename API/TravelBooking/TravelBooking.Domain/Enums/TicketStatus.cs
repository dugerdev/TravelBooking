

namespace TravelBooking.Domain.Enums;

public enum TicketStatus                      //---Bilet Durumlari icin Enum. 

{
    Reserved = 1,                               //---Rezerve Edildi
    Confirmed = 2,                               //---Onaylandi
    CheckedIn = 3,                                  //---Check-in Yapildi
    Cancelled = 4,                                  //---Iptal Edildi
    Used = 5                                        //---Kullanildi

}
