using FluentValidation;
using TaxCalculator.Models.Requests;

namespace TaxCalculator.Models.Validators;

public sealed class ProgressiveBracketRequestValidator : AbstractValidator<ProgressiveBracketRequest>
{
    public ProgressiveBracketRequestValidator()
    {
        RuleFor(x => x.Rate)
            .InclusiveBetween(0, 100)
            .WithMessage("Bracket rate must be between 0 and 100.");

        RuleFor(x => x.Threshold)
            .GreaterThan(0)
            .When(x => x.Threshold.HasValue)
            .WithMessage("Bracket threshold must be greater than 0 when specified.");
    }
}
