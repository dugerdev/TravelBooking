using FluentValidation;
using TravelBooking.Domain.Entities;

namespace TravelBooking.Application.Validators;

public class NewsArticleValidator : AbstractValidator<NewsArticle>
{
    public NewsArticleValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Baslik zorunludur.")
            .MaximumLength(200).WithMessage("Baslik en fazla 200 karakter olabilir.");

        RuleFor(x => x.Summary)
            .NotEmpty().WithMessage("Ozet zorunludur.")
            .MaximumLength(500).WithMessage("Ozet en fazla 500 karakter olabilir.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Icerik zorunludur.");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Kategori zorunludur.")
            .MaximumLength(100).WithMessage("Kategori en fazla 100 karakter olabilir.");

        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("Yazar zorunludur.")
            .MaximumLength(100).WithMessage("Yazar en fazla 100 karakter olabilir.");

        RuleFor(x => x.ViewCount)
            .GreaterThanOrEqualTo(0).WithMessage("Goruntulenme sayisi negatif olamaz.");
    }
}
