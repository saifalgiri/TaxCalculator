using FluentValidation;
using TaxCalculator.Models.Enums;
using TaxCalculator.Models.Requests;

namespace TaxCalculator.Models.Validators;

public sealed class ConfigureTaxRuleRequestValidator : AbstractValidator<ConfigureTaxRuleRequest>
{
    public ConfigureTaxRuleRequestValidator()
    {
        RuleFor(x => x.CountryCode)
            .NotEmpty()
            .WithMessage("CountryCode is required.")
            .Length(2, 3)
            .WithMessage("CountryCode must be 2 or 3 characters (e.g. DE, US, ESP).")
            .Matches("^[A-Za-z]+$")
            .WithMessage("CountryCode must contain only letters.");

        RuleFor(x => x.TaxItems)
            .NotEmpty()
            .WithMessage("At least one tax item must be provided.");

        RuleForEach(x => x.TaxItems)
            .SetValidator(new TaxItemRequestValidator());

        RuleFor(x => x.TaxItems)
            .Must(items => items.Count(i => i.Type == TaxItemType.Progressive) <= 1)
            .WithMessage("Only one Progressive tax item is allowed per country configuration.");
    }
}
