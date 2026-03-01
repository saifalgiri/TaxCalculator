using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TaxCalculator.Database.Entities;
using TaxCalculator.Models.Requests;
using TaxCalculator.Models.Responses;
using TaxCalculator.Domain.Interfaces;

namespace TaxCalculator.API.Controllers;

[ApiController]
[Route("api/tax")]
[Produces("application/json")]
public sealed class TaxController : ControllerBase
{
    private readonly ITaxConfigurationService _configurationService;
    private readonly ITaxCalculationService _calculationService;
    private readonly IMapper _mapper;

    public TaxController(
        ITaxConfigurationService configurationService,
        ITaxCalculationService calculationService,
        IMapper mapper)
    {
        _configurationService = configurationService;
        _calculationService = calculationService;
        _mapper = mapper;
    }

    /// <summary>
    /// Configure or replace the tax rules for a country.
    /// Providing a configuration for an existing country code will replace it entirely.
    /// </summary>
    [HttpPut("configuration")]
    [ProducesResponseType(typeof(ConfigureTaxRuleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Configure(
        [FromBody] ConfigureTaxRuleRequest request,
        CancellationToken cancellationToken)
    {
        // AutoMapper: request -> entity (mapping in API layer, service receives entity)
        var entity = _mapper.Map<CountryTaxConfigurationEntity>(request);

        var savedCountryCode = await _configurationService
            .SaveConfigurationAsync(entity, cancellationToken);

        return Ok(new ConfigureTaxRuleResponse
        {
            CountryCode = savedCountryCode,
            Message = $"Tax configuration for '{savedCountryCode}' saved successfully."
        });
    }

    /// <summary>
    /// Calculate tax for a given country and annual gross salary.
    /// Returns gross salary, taxable base, total taxes, net salary and a per-item breakdown.
    /// </summary>
    [HttpPost("calculate")]
    [ProducesResponseType(typeof(CalculateTaxResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Calculate(
        [FromBody] CalculateTaxRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _calculationService
            .CalculateAsync(request.CountryCode, request.GrossSalary, cancellationToken);

        // AutoMapper: service result -> API response
        var response = _mapper.Map<CalculateTaxResponse>(result);
        return Ok(response);
    }
}
