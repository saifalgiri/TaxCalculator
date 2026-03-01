using FluentValidation;
using TaxCalculator.Models.Requests;

namespace TaxCalculator.Models.Validators;

public sealed class CalculateTaxRequestValidator : AbstractValidator<CalculateTaxRequest>
{
    public CalculateTaxRequestValidator()
    {
        RuleFor(x => x.CountryCode)
            .NotEmpty()
            .WithMessage("CountryCode is required.")
            .Length(2, 3)
            .WithMessage("CountryCode must be 2 or 3 characters (e.g. DE, US, ESP).")
            .Matches("^[A-Za-z]+$")
            .WithMessage("CountryCode must contain only letters.");

        RuleFor(x => x.GrossSalary)
            .GreaterThanOrEqualTo(10000) // TODO: Determine and configure the minimum annual salary threshold for taxation.
            .WithMessage("Gross salary must be greater than or equal 10000 for taxation.");
    }
}
