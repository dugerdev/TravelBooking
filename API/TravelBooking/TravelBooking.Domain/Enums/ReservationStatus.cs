

namespace TravelBooking.Domain.Enums;

public enum ReservationStatus
{
    Pending = 0,                              //---olusturuldu, odeme bekleniyor
    Confirmed = 1,                             //---Odeme alindi, rezervasyon onaylandi
    Cancelled = 2,                           //---kullanici veya sistem tarafindan iptal edildi.
    Completed = 3,                            //---Rezervasyon tamamlandi.
    PaymentFailed = 4                      //---Odeme islemi basarisiz oldu.

}
