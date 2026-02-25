namespace TravelBooking.Domain.Exceptions;

//---Rezervasyon ile ilgili domain ihlalleri icin exception sinifi---//
public class ReservationDomainException : DomainException
{
    public ReservationDomainException(string message) : base(message)          //---Hata mesaji ile exception olusturan constructor---//
    {
    }

    public ReservationDomainException(string message, Exception innerException) : base(message, innerException)  //---Ic exception ile birlikte exception olusturan constructor---//
    {
    }
}

