using FluentValidation;
using TravelBooking.Application.Dtos;
using TravelBooking.Domain.Enums;

namespace TravelBooking.Application.Validators;

/// <summary>
/// Ucus olusturma icin DTO dogrulama kurallari
/// Bu validator, CreateFlightDto nesnesinin gecerli olup olmadigini kontrol eder
/// </summary>
public class CreateFlightDtoValidator : AbstractValidator<CreateFlightDto>
{
    public CreateFlightDtoValidator()
    {
        // Ucus numarasi zorunludur ve en fazla 20 karakter olabilir
        RuleFor(x => x.FlightNumber)
            .NotEmpty().WithMessage("Ucus numarasi zorunludur.")
            .MaximumLength(20).WithMessage("Ucus numarasi en fazla 20 karakter olabilir.");

        // Havayolu sirketi adi zorunludur ve en fazla 100 karakter olabilir
        RuleFor(x => x.AirlineName)
            .NotEmpty().WithMessage("Havayolu sirketi adi zorunludur.")
            .MaximumLength(100).WithMessage("Havayolu sirketi adi en fazla 100 karakter olabilir.");

        // Kalkis havalimani ID'si zorunludur (bos GUID olamaz)
        RuleFor(x => x.DepartureAirportId)
            .NotEmpty().WithMessage("Kalkis havalimani zorunludur.");

        // Varis havalimani ID'si zorunludur ve kalkis havalimanindan farkli olmalidir
        RuleFor(x => x.ArrivalAirportId)
            .NotEmpty().WithMessage("Varis havalimani zorunludur.")
            .NotEqual(x => x.DepartureAirportId)
            .WithMessage("Kalkis ve varis havalimanlari ayni olamaz.");

        // Kalkis zamani zorunludur ve varis zamanindan once olmalidir
        RuleFor(x => x.ScheduledDeparture)
            .NotEmpty().WithMessage("Kalkis zamani zorunludur.")
            .Must((dto, departure) => departure < dto.ScheduledArrival)
            .WithMessage("Kalkis zamani, varis zamanindan once olmalidir.");

        // Varis zamani zorunludur
        RuleFor(x => x.ScheduledArrival)
            .NotEmpty().WithMessage("Varis zamani zorunludur.");

        // Temel fiyat 0'dan buyuk olmalidir
        RuleFor(x => x.BasePriceAmount)
            .GreaterThan(0).WithMessage("Temel fiyat 0'dan buyuk olmalidir.");

        // Para birimi enum degerlerinden biri olmalidir
        RuleFor(x => x.Currency)
            .IsInEnum().WithMessage("Gecerli bir para birimi secilmelidir.");

        // Toplam koltuk sayisi 0'dan buyuk olmalidir
        RuleFor(x => x.TotalSeats)
            .GreaterThan(0).WithMessage("Toplam koltuk sayisi 0'dan buyuk olmalidir.");

        // Ucus tipi enum degerlerinden biri olmalidir
        RuleFor(x => x.FlightType)
            .IsInEnum().WithMessage("Gecerli bir ucus tipi secilmelidir.");

        // Ucus bolgesi enum degerlerinden biri olmalidir
        RuleFor(x => x.FlightRegion)
            .IsInEnum().WithMessage("Gecerli bir ucus bolgesi secilmelidir.");
    }
}

