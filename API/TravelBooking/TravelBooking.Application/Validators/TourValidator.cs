using FluentValidation;
using TravelBooking.Domain.Entities;

namespace TravelBooking.Application.Validators;

public class TourValidator : AbstractValidator<Tour>
{
    public TourValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tur adi zorunludur.")
            .MaximumLength(200).WithMessage("Tur adi en fazla 200 karakter olabilir.");

        RuleFor(x => x.Destination)
            .NotEmpty().WithMessage("Destinasyon zorunludur.")
            .MaximumLength(200).WithMessage("Destinasyon en fazla 200 karakter olabilir.");

        RuleFor(x => x.Duration)
            .GreaterThan(0).WithMessage("Sure 0'dan buyuk olmalidir.");

        RuleFor(x => x.Price)
            .NotNull().WithMessage("Fiyat zorunludur.")
            .Must(price => price != null && price.Amount > 0)
            .WithMessage("Fiyat 0'dan buyuk olmalidir.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Aciklama en fazla 2000 karakter olabilir.");

        RuleFor(x => x.Difficulty)
            .MaximumLength(50).WithMessage("Zorluk seviyesi en fazla 50 karakter olabilir.");

        RuleFor(x => x.MaxGroupSize)
            .GreaterThan(0).WithMessage("Maksimum grup sayisi 0'dan buyuk olmalidir.");

        RuleFor(x => x.Rating)
            .GreaterThanOrEqualTo(0).WithMessage("Rating negatif olamaz.")
            .LessThanOrEqualTo(5).WithMessage("Rating 5'ten buyuk olamaz.");

        RuleFor(x => x.ReviewCount)
            .GreaterThanOrEqualTo(0).WithMessage("Yorum sayisi negatif olamaz.");
    }
}
