namespace Couture.Clients.Contracts.Dtos;

public sealed record ClientSummaryDto(
    Guid Id,
    string Code,
    string FirstName,
    string LastName,
    string FullName,
    string PrimaryPhone,
    int ActiveOrderCount);
