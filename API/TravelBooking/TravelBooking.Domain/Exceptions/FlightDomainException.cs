namespace TravelBooking.Domain.Exceptions;

//---Ucus ile ilgili domain ihlalleri icin exception sinifi---//
public class FlightDomainException : DomainException
{
    public FlightDomainException(string message) : base(message)               //---Hata mesaji ile exception olusturan constructor---//
    {
    }

    public FlightDomainException(string message, Exception innerException) : base(message, innerException)  //---Ic exception ile birlikte exception olusturan constructor---//
    {
    }
}

