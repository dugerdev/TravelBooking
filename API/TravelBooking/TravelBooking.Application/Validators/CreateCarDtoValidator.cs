using FluentValidation;
using TravelBooking.Application.Dtos;

namespace TravelBooking.Application.Validators;

public class CreateCarDtoValidator : AbstractValidator<CreateCarDto>
{
    public CreateCarDtoValidator()
    {
        RuleFor(x => x.Brand)
            .NotEmpty().WithMessage("Marka zorunludur.")
            .MaximumLength(100).WithMessage("Marka en fazla 100 karakter olabilir.");

        RuleFor(x => x.Model)
            .NotEmpty().WithMessage("Model zorunludur.")
            .MaximumLength(100).WithMessage("Model en fazla 100 karakter olabilir.");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Kategori zorunludur.")
            .MaximumLength(50).WithMessage("Kategori en fazla 50 karakter olabilir.");

        RuleFor(x => x.Year)
            .GreaterThanOrEqualTo(1900).WithMessage("Yil 1900'den kucuk olamaz.")
            .LessThanOrEqualTo(DateTime.Now.Year + 1).WithMessage("Yil gelecek yildan buyuk olamaz.");

        RuleFor(x => x.FuelType)
            .NotEmpty().WithMessage("Yakit tipi zorunludur.")
            .MaximumLength(50).WithMessage("Yakit tipi en fazla 50 karakter olabilir.");

        RuleFor(x => x.Transmission)
            .NotEmpty().WithMessage("Vites tipi zorunludur.")
            .MaximumLength(50).WithMessage("Vites tipi en fazla 50 karakter olabilir.");

        RuleFor(x => x.Seats)
            .GreaterThan(0).WithMessage("Koltuk sayisi 0'dan buyuk olmalidir.");

        RuleFor(x => x.Doors)
            .GreaterThan(0).WithMessage("Kapi sayisi 0'dan buyuk olmalidir.");

        RuleFor(x => x.PricePerDay)
            .GreaterThan(0).WithMessage("Gunluk fiyat 0'dan buyuk olmalidir.");

        RuleFor(x => x.Currency)
            .IsInEnum().WithMessage("Gecersiz para birimi.");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Lokasyon zorunludur.")
            .MaximumLength(200).WithMessage("Lokasyon en fazla 200 karakter olabilir.");
    }
}
