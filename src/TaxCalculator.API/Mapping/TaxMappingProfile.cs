using AutoMapper;
using TaxCalculator.Database.Entities;
using TaxCalculator.Models.Enums;
using TaxCalculator.Models.Requests;
using TaxCalculator.Models.Responses;
using TaxCalculator.Domain.Interfaces;

namespace TaxCalculator.API.Mapping;

public sealed class TaxMappingProfile : Profile
{
    public TaxMappingProfile()
    {
        // ── ConfigureTaxRuleRequest → CountryTaxConfigurationEntity ──────────
        // CountryCode is mapped into the Country navigation object.
        // CountryId is resolved by TaxConfigurationService via DB upsert — ignored here.
        CreateMap<ConfigureTaxRuleRequest, CountryTaxConfigurationEntity>()
            .ForMember(dest => dest.Country,
                opt => opt.MapFrom(src => new CountryEntity
                {
                    Code = src.CountryCode.ToUpperInvariant()
                }))
            .ForMember(dest => dest.CountryId,
                opt => opt.Ignore())
            .ForMember(dest => dest.Id,
                opt => opt.Ignore())
            .ForMember(dest => dest.TaxItems,
                opt => opt.MapFrom(src => src.TaxItems));

        CreateMap<TaxItemRequest, TaxItemEntity>()
            .ForMember(dest => dest.Type,
                opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.Amount,
                opt => opt.MapFrom(src => src.Amount))
            .ForMember(dest => dest.Rate,
                // Percentage (e.g. 20) → fraction (0.20) for storage
                opt => opt.MapFrom(src =>
                    src.Rate.HasValue ? (decimal?)(src.Rate.Value / 100m) : null))
            .ForMember(dest => dest.Brackets,
                opt => opt.MapFrom(src =>
                    src.Type == TaxItemType.Progressive ? src.Brackets : null))
            .ForMember(dest => dest.Id,
                opt => opt.Ignore())
            .ForMember(dest => dest.CountryTaxConfigurationId,
                opt => opt.Ignore())
            .ForMember(dest => dest.CountryTaxConfiguration,
                opt => opt.Ignore());

        CreateMap<ProgressiveBracketRequest, ProgressiveBracketEntity>()
            .ForMember(dest => dest.Rate,
                // Percentage → fraction
                opt => opt.MapFrom(src => src.Rate / 100m))
            .ForMember(dest => dest.Threshold,
                opt => opt.MapFrom(src => src.Threshold))
            .ForMember(dest => dest.Id,
                opt => opt.Ignore())
            .ForMember(dest => dest.TaxItemId,
                opt => opt.Ignore())
            .ForMember(dest => dest.TaxItem,
                opt => opt.Ignore());

        // ── TaxCalculationResult → CalculateTaxResponse ───────────────────────
        CreateMap<TaxCalculationResult, CalculateTaxResponse>()
            .ForMember(dest => dest.Breakdown,
                opt => opt.MapFrom(src => src.Breakdown));

        CreateMap<TaxItemResult, TaxBreakdownItemResponse>()
            .ForMember(dest => dest.Type,
                opt => opt.MapFrom(src => Enum.Parse<TaxItemType>(src.Type, true)));
    }
}
