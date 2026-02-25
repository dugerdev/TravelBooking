using FluentValidation;
using TravelBooking.Domain.Entities;

namespace TravelBooking.Application.Validators;

//---Ucus dogrulama kurallari---//
public class FlightValidator : AbstractValidator<Flight>
{
    public FlightValidator()
    {
        RuleFor(x => x.FlightNumber)
            .NotEmpty().WithMessage("Ucus numarasi zorunludur.")
            .MaximumLength(20).WithMessage("Ucus numarasi en fazla 20 karakter olabilir.");

        RuleFor(x => x.AirlineName)
            .NotEmpty().WithMessage("Havayolu sirketi adi zorunludur.")
            .MaximumLength(100).WithMessage("Havayolu sirketi adi en fazla 100 karakter olabilir.");

        RuleFor(x => x.ScheduledDeparture)
            .LessThan(x => x.ScheduledArrival)
            .WithMessage("Kalkis zamani, varis zamanindan once olmalidir.");

        RuleFor(x => x.BasePrice)
            .NotNull().WithMessage("Temel fiyat zorunludur.")
            .Must(price => price != null && price.Amount > 0)
            .WithMessage("Temel fiyat 0'dan buyuk olmalidir.");

        RuleFor(x => x.TotalSeats)
            .GreaterThan(0).WithMessage("Toplam koltuk sayisi 0'dan buyuk olmalidir.");

        RuleFor(x => x.AvailableSeats)
            .GreaterThanOrEqualTo(0).WithMessage("Musait koltuk sayisi negatif olamaz.")
            .LessThanOrEqualTo(x => x.TotalSeats)
            .WithMessage("Musait koltuk sayisi toplam koltuk sayisindan fazla olamaz.");
    }
}

