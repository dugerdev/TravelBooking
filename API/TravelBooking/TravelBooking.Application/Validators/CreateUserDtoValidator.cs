using FluentValidation;
using TravelBooking.Application.Dtos;

namespace TravelBooking.Application.Validators;

/// <summary>
/// Kullanici olusturma icin DTO dogrulama kurallari
/// Bu validator, CreateUserDto nesnesinin gecerli olup olmadigini kontrol eder
/// </summary>
public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserDtoValidator()
    {
        // Kullanici adi zorunludur, en az 3 en fazla 50 karakter olabilir
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters.")
            .MaximumLength(50).WithMessage("Username can be at most 50 characters.");

        // E-posta adresi zorunludur ve gecerli bir e-posta formatinda olmalidir
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("Enter a valid email address.");

        // Sifre zorunludur ve en az 8 karakter olmalidir
        // Not: Sifre karmasikligi Identity framework tarafindan kontrol edilir
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.");

        // Telefon numarasi zorunlu degil ama verilmisse uluslararasi formatta olmalidir
        // Ornek: +905551234567 (E.164 formati)
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$").When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Enter a valid phone number format.");
    }
}

