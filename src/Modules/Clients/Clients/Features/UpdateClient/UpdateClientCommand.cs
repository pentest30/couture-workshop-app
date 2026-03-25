using Mediator;
namespace Couture.Clients.Features.UpdateClient;
public sealed record UpdateClientCommand(Guid ClientId, string? FirstName, string? LastName, string? PrimaryPhone,
    string? SecondaryPhone, string? Address, DateOnly? DateOfBirth, string? Notes) : ICommand;
