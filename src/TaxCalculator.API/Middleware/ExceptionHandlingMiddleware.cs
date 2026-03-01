using System.Net;
using System.Text.Json;
using TaxCalculator.Domain.Exceptions;

namespace TaxCalculator.API.Middleware;

/// <summary>
/// Centralises exception-to-HTTP response mapping.
/// Controllers stay clean — no try/catch blocks needed.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail) = exception switch
        {
            CountryConfigurationNotFoundException ex =>
                (HttpStatusCode.NotFound, "Country Configuration Not Found", ex.Message),

            InvalidTaxConfigurationException ex =>
                (HttpStatusCode.BadRequest, "Invalid Tax Configuration", ex.Message),

            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.",
                  "Please try again later.")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception");
        else
            _logger.LogWarning(exception, "Handled exception: {Title}", title);

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new
        {
            type = $"https://httpstatuses.io/{(int)statusCode}",
            title,
            status = (int)statusCode,
            detail
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
