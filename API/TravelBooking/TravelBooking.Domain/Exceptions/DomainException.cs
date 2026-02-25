namespace TravelBooking.Domain.Exceptions;

//---Domain katmaninda olusabilecek tum exception'larin base sinifi---//
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)                  //---Hata mesaji ile exception olusturan constructor---//
    {
    }

    protected DomainException(string message, Exception innerException) : base(message, innerException)  //---Ic exception ile birlikte exception olusturan constructor---//
    {
    }
}
