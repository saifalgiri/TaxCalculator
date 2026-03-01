using FluentValidation;
using TaxCalculator.Models.Enums;
using TaxCalculator.Models.Requests;

namespace TaxCalculator.Models.Validators;

public sealed class TaxItemRequestValidator : AbstractValidator<TaxItemRequest>
{
    public TaxItemRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Tax item Name is required.");

        When(x => x.Type == TaxItemType.Fixed, () =>
        {
            RuleFor(x => x.Amount)
                .NotNull()
                .WithMessage("Fixed tax item requires an Amount.")
                .GreaterThanOrEqualTo(0)
                .WithMessage("Amount must be non-negative.");
        });

        When(x => x.Type == TaxItemType.FlatRate, () =>
        {
            RuleFor(x => x.Rate)
                .NotNull()
                .WithMessage("FlatRate tax item requires a Rate.")
                .InclusiveBetween(0, 100)
                .WithMessage("Rate must be between 0 and 100.");
        });

        When(x => x.Type == TaxItemType.Progressive, () =>
        {
            RuleFor(x => x.Brackets)
                .NotNull()
                .WithMessage("Progressive tax item requires Brackets.")
                .Must(b => b != null && b.Count > 0)
                .WithMessage("Progressive tax item requires at least one bracket.");

            RuleForEach(x => x.Brackets!)
                .SetValidator(new ProgressiveBracketRequestValidator());

            RuleFor(x => x.Brackets!)
                .Must(brackets => brackets.Count(b => b.Threshold == null) == 1)
                .WithMessage("Progressive tax item must have exactly one open-ended bracket (null threshold).");

            RuleFor(x => x.Brackets!)
                .Must(brackets =>
                {
                    var openIdx = brackets.FindIndex(b => b.Threshold == null);
                    return openIdx == brackets.Count - 1;
                })
                .WithMessage("The open-ended bracket (null threshold) must be the last bracket.");
        });
    }
}
