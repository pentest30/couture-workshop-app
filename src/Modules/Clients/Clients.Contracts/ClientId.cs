namespace Couture.Clients.Contracts;
public readonly record struct ClientId(Guid Value)
{
    public static ClientId From(Guid value) => new(value);
    public static ClientId NewClientId() => new(Guid.NewGuid());
}
