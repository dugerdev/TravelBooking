

namespace TravelBooking.Domain.Enums;

public enum TransactionType


{
    Payment = 1,                            //--- Normal odeme islemi
            
    Refund = 2,                            //--- Iade islemi 

    Chargeback = 3                        //--- Ters ibraz islemi (itiraz)    

}
