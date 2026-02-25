using FluentValidation;
using TravelBooking.Application.Dtos;

namespace TravelBooking.Application.Validators;

public class CreateHotelDtoValidator : AbstractValidator<CreateHotelDto>
{
    public CreateHotelDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Otel adi zorunludur.")
            .MaximumLength(200).WithMessage("Otel adi en fazla 200 karakter olabilir.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("Sehir zorunludur.")
            .MaximumLength(100).WithMessage("Sehir en fazla 100 karakter olabilir.");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Ulke zorunludur.")
            .MaximumLength(100).WithMessage("Ulke en fazla 100 karakter olabilir.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Adres zorunludur.")
            .MaximumLength(500).WithMessage("Adres en fazla 500 karakter olabilir.");

        RuleFor(x => x.StarRating)
            .GreaterThanOrEqualTo(1).WithMessage("Yildiz sayisi en az 1 olmalidir.")
            .LessThanOrEqualTo(5).WithMessage("Yildiz sayisi en fazla 5 olabilir.");

        RuleFor(x => x.PricePerNight)
            .GreaterThan(0).WithMessage("Gecelik fiyat 0'dan buyuk olmalidir.");

        RuleFor(x => x.Currency)
            .IsInEnum().WithMessage("Gecersiz para birimi.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Aciklama en fazla 2000 karakter olabilir.");
    }
}
