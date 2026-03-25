namespace Couture.Dashboard.Contracts.Dtos;

public sealed record MonthlyHistogramDto(List<MonthlyBarDto> Months);
public sealed record MonthlyBarDto(string Month, int Simple, int Embroidered, int Beaded, int Mixed);

public sealed record StatusDistributionDto(List<StatusSliceDto> Statuses);
public sealed record StatusSliceDto(string Status, string Label, string Color, int Count, decimal Percentage);

public sealed record WorkTypeDistributionDto(List<WorkTypeSliceDto> WorkTypes);
public sealed record WorkTypeSliceDto(string Type, string Label, int Count, decimal Percentage);

public sealed record RevenueTrendDto(List<QuarterRevenueDto> Quarters);
public sealed record QuarterRevenueDto(string Label, decimal Revenue);

public sealed record DelayByArtisanDto(List<ArtisanDelayDto> Artisans);
public sealed record ArtisanDelayDto(string Name, decimal AvgDelayDays);
