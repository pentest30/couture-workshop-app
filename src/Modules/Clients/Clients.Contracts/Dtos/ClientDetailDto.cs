namespace Couture.Clients.Contracts.Dtos;

public sealed record ClientDetailDto(
    Guid Id,
    string Code,
    string FirstName,
    string LastName,
    string FullName,
    string PrimaryPhone,
    string? SecondaryPhone,
    string? Address,
    DateOnly? DateOfBirth,
    string? Notes,
    List<MeasurementDto> CurrentMeasurements,
    ClientStatsDto Stats,
    DateTimeOffset CreatedAt);

public sealed record MeasurementDto(
    Guid FieldId,
    string FieldName,
    string Unit,
    decimal Value,
    DateTimeOffset MeasuredAt);

public sealed record ClientStatsDto(
    int TotalOrders,
    decimal TotalAmountCollected,
    DateOnly? LastVisitDate,
    int ActiveOrders);
