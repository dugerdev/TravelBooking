using FluentValidation;
using TravelBooking.Application.Dtos;

namespace TravelBooking.Application.Validators;

//---Bilet olusturma icin DTO dogrulama kurallari---//
public class CreateTicketDtoValidator : AbstractValidator<CreateTicketDto>
{
    public CreateTicketDtoValidator()
    {
        RuleFor(x => x.FlightId)
            .NotEmpty().WithMessage("Ucus kimligi zorunludur.");

        // NOT: ReservationId ve PassengerId, CreateReservation akisinda backend tarafindan set edilir
        // Frontend'den gonderilmesi zorunlu degil (Guid.Empty kabul edilir)
        // Sadece FlightId, Email, ContactPhoneNumber ve diger bilet bilgileri zorunlu

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta adresi zorunludur.")
            .EmailAddress().WithMessage("Gecerli bir e-posta adresi giriniz.")
            .MaximumLength(200).WithMessage("E-posta adresi en fazla 200 karakter olabilir.");

        RuleFor(x => x.ContactPhoneNumber)
            .NotEmpty().WithMessage("Iletisim telefon numarasi zorunludur.")
            .MaximumLength(20).WithMessage("Telefon numarasi en fazla 20 karakter olabilir.");

        RuleFor(x => x.SeatClass)
            .IsInEnum().WithMessage("Gecerli bir koltuk sinifi secilmelidir.");

        RuleFor(x => x.BaggageOption)
            .IsInEnum().WithMessage("Gecerli bir bagaj secenegi secilmelidir.");

        RuleFor(x => x.TicketPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Bilet fiyati negatif olamaz.");

        RuleFor(x => x.BaggageFee)
            .GreaterThanOrEqualTo(0).WithMessage("Bagaj ucreti negatif olamaz.");

        RuleFor(x => x.SeatNumber)
            .MaximumLength(10).When(x => !string.IsNullOrEmpty(x.SeatNumber))
            .WithMessage("Koltuk numarasi en fazla 10 karakter olabilir.");
    }
}
