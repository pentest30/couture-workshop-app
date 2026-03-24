namespace Couture.Finance.Contracts;
public readonly record struct ReceiptId(Guid Value)
{
    public static ReceiptId From(Guid value) => new(value);
    public static ReceiptId NewReceiptId() => new(Guid.NewGuid());
}
