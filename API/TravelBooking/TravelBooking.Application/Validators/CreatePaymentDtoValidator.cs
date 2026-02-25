using FluentValidation;
using TravelBooking.Application.Dtos;

namespace TravelBooking.Application.Validators;

//---Odeme olusturma icin DTO dogrulama kurallari---//
public class CreatePaymentDtoValidator : AbstractValidator<CreatePaymentDto>
{
    public CreatePaymentDtoValidator()
    {
        // NOT: ReservationId, CreateReservation akisinda backend tarafindan set edilir
        // Frontend'den gonderilmesi zorunlu degil (Guid.Empty kabul edilir)
        
        RuleFor(x => x.TransactionAmount)
            .GreaterThan(0).WithMessage("Islem tutari sifirdan buyuk olmalidir.");

        RuleFor(x => x.Currency)
            .IsInEnum().WithMessage("Gecerli bir para birimi secilmelidir.");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("Gecerli bir odeme yontemi secilmelidir.");

        RuleFor(x => x.TransactionId)
            .NotEmpty().WithMessage("Islem kimligi zorunludur.")
            .MaximumLength(100).WithMessage("Islem kimligi en fazla 100 karakter olabilir.");

        RuleFor(x => x.TransactionType)
            .IsInEnum().WithMessage("Gecerli bir islem turu secilmelidir.");
    }
}
