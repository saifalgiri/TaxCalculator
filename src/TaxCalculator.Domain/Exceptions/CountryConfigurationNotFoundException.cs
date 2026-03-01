namespace TaxCalculator.Domain.Exceptions;

public sealed class CountryConfigurationNotFoundException : Exception
{
    public string CountryCode { get; }

    public CountryConfigurationNotFoundException(string countryCode)
        : base($"No tax configuration found for country '{countryCode}'.") =>
        CountryCode = countryCode;
}
