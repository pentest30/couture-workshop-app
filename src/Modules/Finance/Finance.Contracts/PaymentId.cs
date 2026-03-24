namespace Couture.Finance.Contracts;
public readonly record struct PaymentId(Guid Value)
{
    public static PaymentId From(Guid value) => new(value);
    public static PaymentId NewPaymentId() => new(Guid.NewGuid());
}
