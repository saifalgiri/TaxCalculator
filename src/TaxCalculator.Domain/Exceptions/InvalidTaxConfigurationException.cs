namespace TaxCalculator.Domain.Exceptions;

public sealed class InvalidTaxConfigurationException : Exception
{
    public InvalidTaxConfigurationException(string message)
        : base(message) { }

    public InvalidTaxConfigurationException(string message, Exception inner)
        : base(message, inner) { }
}
