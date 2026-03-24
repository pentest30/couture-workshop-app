namespace Couture.Orders.Contracts.Dtos;

public sealed record PagedResultDto<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize);
