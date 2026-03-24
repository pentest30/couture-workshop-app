using Couture.Orders.Contracts;
using Couture.Orders.Contracts.Events;
using Couture.SharedKernel;

namespace Couture.Orders.Domain;

public sealed class Order : AggregateRoot
{
    public OrderId Id { get; private set; }
    public string Code { get; private set; } = default!;
    public Guid ClientId { get; private set; }
    public OrderStatus Status { get; private set; } = OrderStatus.Recue;
    public WorkType WorkType { get; private set; } = WorkType.Simple;

    public string? Description { get; private set; }
    public string? Fabric { get; private set; }
    public string? TechnicalNotes { get; private set; }

    // Embroidery-specific
    public string? EmbroideryStyle { get; private set; }
    public string? ThreadColors { get; private set; }
    public string? Density { get; private set; }
    public string? EmbroideryZone { get; private set; }

    // Beading-specific
    public string? BeadType { get; private set; }
    public string? Arrangement { get; private set; }
    public string? AffectedZones { get; private set; }

    // Planning
    public DateOnly ReceptionDate { get; private set; }
    public DateOnly ExpectedDeliveryDate { get; private set; }
    public DateOnly? ActualDeliveryDate { get; private set; }
    public decimal TotalPrice { get; private set; }

    // Artisan assignments
    public Guid? AssignedTailorId { get; private set; }
    public Guid? AssignedEmbroidererId { get; private set; }
    public Guid? AssignedBeaderId { get; private set; }

    // Delivery
    public string? DeliveryWithUnpaidReason { get; private set; }
    public bool HasUnpaidBalance { get; private set; }

    // Navigation
    private readonly List<StatusTransition> _transitions = [];
    public IReadOnlyList<StatusTransition> Transitions => _transitions.AsReadOnly();

    private readonly List<OrderPhoto> _photos = [];
    public IReadOnlyList<OrderPhoto> Photos => _photos.AsReadOnly();

    private Order() { } // EF Core

    public static Order Create(
        string code,
        Guid clientId,
        WorkType workType,
        DateOnly expectedDeliveryDate,
        decimal totalPrice,
        string? description = null,
        string? fabric = null,
        string? technicalNotes = null,
        Guid? assignedTailorId = null,
        Guid? assignedEmbroidererId = null,
        Guid? assignedBeaderId = null)
    {
        if (totalPrice <= 0)
            throw new InvalidOperationException("Total price must be greater than zero.");

        var order = new Order
        {
            Id = OrderId.From(Guid.NewGuid()),
            Code = code,
            ClientId = clientId,
            WorkType = workType,
            Status = OrderStatus.Recue,
            ReceptionDate = DateOnly.FromDateTime(DateTime.UtcNow),
            ExpectedDeliveryDate = expectedDeliveryDate,
            TotalPrice = totalPrice,
            Description = description,
            Fabric = fabric,
            TechnicalNotes = technicalNotes,
            AssignedTailorId = assignedTailorId,
            AssignedEmbroidererId = assignedEmbroidererId,
            AssignedBeaderId = assignedBeaderId,
        };

        order.AddTransition(null, OrderStatus.Recue, null, "System");
        order.RaiseDomainEvent(new OrderCreatedEvent(order.Id, order.Code, workType.Name, assignedTailorId));

        return order;
    }

    public void ChangeStatus(OrderStatus newStatus, Guid changedByUserId, string? reason = null)
    {
        if (Status.IsTerminal)
            throw new InvalidOperationException("Cannot change status of a delivered order.");

        if (!Status.CanTransitionTo(newStatus, WorkType))
            throw new InvalidOperationException($"Cannot transition from {Status.Name} to {newStatus.Name} for work type {WorkType.Name}.");

        if (newStatus == OrderStatus.EnCours && AssignedTailorId is null)
            throw new InvalidOperationException("A tailor must be assigned before moving to En Cours.");

        if (newStatus == OrderStatus.Broderie && AssignedEmbroidererId is null)
            throw new InvalidOperationException("An embroiderer must be assigned before moving to Broderie.");

        if (newStatus == OrderStatus.Perlage && AssignedBeaderId is null)
            throw new InvalidOperationException("A beader must be assigned before moving to Perlage.");

        if (newStatus == OrderStatus.Retouche && string.IsNullOrWhiteSpace(reason))
            throw new InvalidOperationException("A reason is required for alteration (Retouche).");

        var previousStatus = Status;
        Status = newStatus;

        AddTransition(previousStatus, newStatus, reason, changedByUserId.ToString());

        if (newStatus == OrderStatus.Livree)
        {
            RaiseDomainEvent(new OrderDeliveredEvent(Id, Code, HasUnpaidBalance, DeliveryWithUnpaidReason));
        }
        else
        {
            RaiseDomainEvent(new StatusChangedEvent(
                Id, Code, previousStatus.Name, newStatus.Name, reason, changedByUserId,
                AssignedEmbroidererId, AssignedBeaderId));
        }
    }

    public void MarkAsDelivered(Guid changedByUserId, DateOnly actualDeliveryDate, decimal outstandingBalance, string? unpaidReason = null)
    {
        if (outstandingBalance > 0 && string.IsNullOrWhiteSpace(unpaidReason))
            throw new InvalidOperationException("A reason is required when delivering with unpaid balance.");

        ActualDeliveryDate = actualDeliveryDate;
        HasUnpaidBalance = outstandingBalance > 0;
        DeliveryWithUnpaidReason = unpaidReason;

        ChangeStatus(OrderStatus.Livree, changedByUserId, unpaidReason);
    }

    public void Update(
        DateOnly? expectedDeliveryDate = null,
        decimal? totalPrice = null,
        string? technicalNotes = null,
        Guid? assignedTailorId = null,
        Guid? assignedEmbroidererId = null,
        Guid? assignedBeaderId = null)
    {
        if (Status.IsTerminal)
            throw new InvalidOperationException("Cannot modify a delivered order.");

        if (expectedDeliveryDate.HasValue) ExpectedDeliveryDate = expectedDeliveryDate.Value;
        if (totalPrice.HasValue && totalPrice.Value > 0) TotalPrice = totalPrice.Value;
        if (technicalNotes is not null) TechnicalNotes = technicalNotes;
        if (assignedTailorId.HasValue) AssignedTailorId = assignedTailorId.Value;
        if (assignedEmbroidererId.HasValue) AssignedEmbroidererId = assignedEmbroidererId.Value;
        if (assignedBeaderId.HasValue) AssignedBeaderId = assignedBeaderId.Value;
    }

    public void AddPhoto(string fileName, string storagePath, Guid uploadedBy)
    {
        _photos.Add(OrderPhoto.Create(Id, fileName, storagePath, uploadedBy));
    }

    private void AddTransition(OrderStatus? from, OrderStatus to, string? reason, string transitionedBy)
    {
        _transitions.Add(StatusTransition.Create(Id, from, to, reason, transitionedBy));
    }
}
