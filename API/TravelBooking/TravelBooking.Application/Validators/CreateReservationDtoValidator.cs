using FluentValidation;
using TravelBooking.Application.Dtos;

namespace TravelBooking.Application.Validators;

/// <summary>
/// Rezervasyon olusturma icin DTO dogrulama kurallari
/// Bu validator, CreateReservationDto nesnesinin gecerli olup olmadigini kontrol eder
/// </summary>
public class CreateReservationDtoValidator : AbstractValidator<CreateReservationDto>
{
    public CreateReservationDtoValidator()
    {
        // NOT: AppUserId controller'da JWT'den set edilir, bu yuzden frontend'den zorunlu degil
        // Frontend bos birakabilir, backend dolduracak
        
        // Toplam fiyat negatif olamaz (0 veya pozitif olmali)
        RuleFor(x => x.TotalPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Toplam fiyat negatif olamaz.");

        // Para birimi enum degerlerinden biri olmalidir
        RuleFor(x => x.Currency)
            .IsInEnum().WithMessage("Gecerli bir para birimi secilmelidir.");

        // PNR kodu verilmisse, en fazla 10 karakter olabilir ve sadece buyuk harf/rakam icerebilir
        RuleFor(x => x.PNR)
            .MaximumLength(10).WithMessage("PNR kodu en fazla 10 karakter olabilir.")
            .Matches("^[A-Z0-9]*$").When(x => !string.IsNullOrEmpty(x.PNR))
            .WithMessage("PNR kodu sadece buyuk harf ve rakam icerebilir.");

        // Biletler icin validasyon
        RuleForEach(x => x.Tickets)
            .SetValidator(new CreateTicketDtoValidator());

        // Odeme icin validasyon (varsa)
        RuleFor(x => x.Payment!)
            .SetValidator(new CreatePaymentDtoValidator())
            .When(x => x.Payment != null);
    }
}

