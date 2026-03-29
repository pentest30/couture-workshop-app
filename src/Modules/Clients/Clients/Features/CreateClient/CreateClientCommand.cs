using Mediator;

namespace Couture.Clients.Features.CreateClient;

public sealed record CreateClientCommand(
    string FirstName,
    string LastName,
    string PrimaryPhone,
    string? SecondaryPhone,
    string? Address,
    DateOnly? DateOfBirth,
    string? Notes,
    bool ConfirmDuplicate = false) : ICommand<CreateClientResult>;

public sealed record CreateClientResult(Guid Id, string Code);
