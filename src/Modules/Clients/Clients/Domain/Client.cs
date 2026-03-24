using Couture.Clients.Contracts;
using Couture.SharedKernel;

namespace Couture.Clients.Domain;

public sealed class Client : AggregateRoot
{
    public ClientId Id { get; private set; }
    public string Code { get; private set; } = default!;
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string PrimaryPhone { get; private set; } = default!;
    public string? SecondaryPhone { get; private set; }
    public string? Address { get; private set; }
    public DateOnly? DateOfBirth { get; private set; }
    public string? Notes { get; private set; }

    private Client() { }

    public static Client Create(string code, string firstName, string lastName, string primaryPhone,
        string? secondaryPhone = null, string? address = null, DateOnly? dateOfBirth = null, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("First name is required.");
        if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Last name is required.");
        if (string.IsNullOrWhiteSpace(primaryPhone)) throw new ArgumentException("Primary phone is required.");

        return new Client
        {
            Id = ClientId.From(Guid.NewGuid()),
            Code = code,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            PrimaryPhone = primaryPhone.Trim(),
            SecondaryPhone = secondaryPhone?.Trim(),
            Address = address?.Trim(),
            DateOfBirth = dateOfBirth,
            Notes = notes?.Trim(),
        };
    }

    public void Update(string? firstName = null, string? lastName = null, string? primaryPhone = null,
        string? secondaryPhone = null, string? address = null, DateOnly? dateOfBirth = null, string? notes = null)
    {
        if (firstName is not null) FirstName = firstName.Trim();
        if (lastName is not null) LastName = lastName.Trim();
        if (primaryPhone is not null) PrimaryPhone = primaryPhone.Trim();
        if (secondaryPhone is not null) SecondaryPhone = secondaryPhone.Trim();
        if (address is not null) Address = address.Trim();
        if (dateOfBirth.HasValue) DateOfBirth = dateOfBirth.Value;
        if (notes is not null) Notes = notes.Trim();
    }

    public string FullName => $"{FirstName} {LastName}";
}
