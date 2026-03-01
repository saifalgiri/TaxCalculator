using Microsoft.Extensions.DependencyInjection;
using TaxCalculator.Domain.Services;
using TaxCalculator.Domain.Interfaces;
using TaxCalculator.Domain.TaxCalculators;

namespace TaxCalculator.Domain;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTaxServices(this IServiceCollection services)
    {
        // Typed calculators (stateless — singleton is safe)
        services.AddSingleton<FixedTaxCalculator>();
        services.AddSingleton<FlatRateTaxCalculator>();
        services.AddSingleton<ProgressiveTaxCalculator>();

        // Typed tax services — each owns its DB scope (scoped matches DbContext lifetime)
        services.AddScoped<IFixedTaxService, FixedTaxService>();
        services.AddScoped<IFlatRateTaxService, FlatRateTaxService>();
        services.AddScoped<IProgressiveTaxService, ProgressiveTaxService>();

        // Orchestrators
        services.AddScoped<ITaxConfigurationService, TaxConfigurationService>();
        services.AddScoped<ITaxCalculationService, TaxCalculationService>();

        return services;
    }
}
