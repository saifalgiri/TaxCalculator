using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using TaxCalculator.API.Mapping;
using TaxCalculator.API.Middleware;
using TaxCalculator.Database.Context;
using TaxCalculator.Models.Validators;
using TaxCalculator.Domain;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<TaxDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=taxcalculator.db"));

// ── Tax Services ──────────────────────────────────────────────────────────────
builder.Services.AddTaxServices();

// ── AutoMapper ────────────────────────────────────────────────────────────────
builder.Services.AddAutoMapper(typeof(TaxMappingProfile));

// ── Controllers + JSON ────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// ── FluentValidation ─────────────────────────────────────────────────────────
// Validators live in the Models project — scanned from there
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<ConfigureTaxRuleRequestValidator>();

// ── Swagger ───────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Tax Calculator API",
        Version = "v1",
        Description = "Configure country tax rules and calculate annual taxes with itemised breakdown."
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

// ── Pipeline ──────────────────────────────────────────────────────────────────
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TaxDbContext>();
    db.Database.EnsureCreated();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tax Calculator API v1");
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

public partial class Program { }
