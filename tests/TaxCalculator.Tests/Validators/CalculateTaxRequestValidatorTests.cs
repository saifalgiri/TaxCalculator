using FluentValidation.TestHelper;
using TaxCalculator.Models.Requests;
using TaxCalculator.Models.Validators;
using Xunit;

namespace TaxCalculator.Tests.Validators;

public sealed class CalculateTaxRequestValidatorTests
{
    private readonly CalculateTaxRequestValidator _sut = new();

    [Fact]
    public void ValidRequest_PassesValidation()
    {
        var r = _sut.TestValidate(new CalculateTaxRequest { CountryCode = "DE", GrossSalary = 50_000m });
        r.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void NegativeGrossSalary_FailsValidation()
    {
        var r = _sut.TestValidate(new CalculateTaxRequest { CountryCode = "DE", GrossSalary = -1m });
        r.ShouldHaveValidationErrorFor(x => x.GrossSalary);
    }

    [Fact]
    public void EmptyCountryCode_FailsValidation()
    {
        var r = _sut.TestValidate(new CalculateTaxRequest { CountryCode = "", GrossSalary = 50_000m });
        r.ShouldHaveValidationErrorFor(x => x.CountryCode);
    }

    [Theory]
    [InlineData("D")]
    [InlineData("DEUS")]
    [InlineData("D1")]
    public void CountryCode_InvalidFormat_FailsValidation(string code)
    {
        var r = _sut.TestValidate(new CalculateTaxRequest { CountryCode = code, GrossSalary = 50_000m });
        r.ShouldHaveValidationErrorFor(x => x.CountryCode);
    }
}
