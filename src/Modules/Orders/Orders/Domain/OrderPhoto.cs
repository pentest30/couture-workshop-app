using Couture.Orders.Contracts;

namespace Couture.Orders.Domain;

public sealed class OrderPhoto
{
    public OrderPhotoId Id { get; private set; }
    public OrderId OrderId { get; private set; }
    public string FileName { get; private set; } = default!;
    public string StoragePath { get; private set; } = default!;
    public DateTimeOffset UploadedAt { get; private set; }
    public Guid UploadedBy { get; private set; }

    private OrderPhoto() { }

    internal static OrderPhoto Create(OrderId orderId, string fileName, string storagePath, Guid uploadedBy)
    {
        return new OrderPhoto
        {
            Id = OrderPhotoId.From(Guid.NewGuid()),
            OrderId = orderId,
            FileName = fileName,
            StoragePath = storagePath,
            UploadedAt = DateTimeOffset.UtcNow,
            UploadedBy = uploadedBy,
        };
    }
}
