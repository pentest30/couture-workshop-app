using Mediator;

namespace Couture.Clients.Features.CreateClient;

public sealed record CreateClientCommand(
    string FirstName,
    string LastName,
    string PrimaryPhone,
    string? SecondaryPhone,
    string? Address,
    DateOnly? DateOfBirth,
    string? Notes) : ICommand<CreateClientResult>;

public sealed record CreateClientResult(Guid ClientId, string Code);
