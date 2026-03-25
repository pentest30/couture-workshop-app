using Couture.Catalog.Contracts;
using Couture.SharedKernel;

namespace Couture.Catalog.Domain;

public sealed class Model : AuditableEntity
{
    public ModelId Id { get; private set; }
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public ModelCategory Category { get; private set; } = default!;
    public string WorkType { get; private set; } = default!;
    public decimal BasePrice { get; private set; }
    public string? Description { get; private set; }
    public int EstimatedDays { get; private set; }
    public bool IsPublic { get; private set; }

    private readonly List<ModelPhoto> _photos = [];
    public IReadOnlyList<ModelPhoto> Photos => _photos.AsReadOnly();

    private readonly List<ModelFabric> _modelFabrics = [];
    public IReadOnlyList<ModelFabric> ModelFabrics => _modelFabrics.AsReadOnly();

    private Model() { }

    public static Model Create(string code, string name, ModelCategory category, string workType,
        decimal basePrice, int estimatedDays, bool isPublic, string? description = null)
    {
        return new Model
        {
            Id = ModelId.From(Guid.NewGuid()),
            Code = code,
            Name = name,
            Category = category,
            WorkType = workType,
            BasePrice = basePrice,
            EstimatedDays = estimatedDays,
            IsPublic = isPublic,
            Description = description,
        };
    }

    public void Update(string? name = null, ModelCategory? category = null, string? workType = null,
        decimal? basePrice = null, int? estimatedDays = null, bool? isPublic = null, string? description = null)
    {
        if (name is not null) Name = name;
        if (category is not null) Category = category;
        if (workType is not null) WorkType = workType;
        if (basePrice.HasValue) BasePrice = basePrice.Value;
        if (estimatedDays.HasValue) EstimatedDays = estimatedDays.Value;
        if (isPublic.HasValue) IsPublic = isPublic.Value;
        if (description is not null) Description = description;
    }

    public ModelPhoto AddPhoto(string fileName, string storagePath, int sortOrder)
    {
        var photo = ModelPhoto.Create(Id, fileName, storagePath, sortOrder);
        _photos.Add(photo);
        return photo;
    }

    public void RemovePhoto(Guid photoId)
    {
        var photo = _photos.FirstOrDefault(p => p.Id == photoId);
        if (photo is not null) _photos.Remove(photo);
    }

    public void LinkFabric(FabricId fabricId)
    {
        if (_modelFabrics.Any(mf => mf.FabricId == fabricId)) return;
        _modelFabrics.Add(ModelFabric.Create(Id, fabricId));
    }

    public void Deactivate() => IsActive = false;

    public void UnlinkFabric(FabricId fabricId)
    {
        var link = _modelFabrics.FirstOrDefault(mf => mf.FabricId == fabricId);
        if (link is not null) _modelFabrics.Remove(link);
    }
}

public sealed class ModelPhoto
{
    public Guid Id { get; private set; }
    public ModelId ModelId { get; private set; }
    public string FileName { get; private set; } = default!;
    public string StoragePath { get; private set; } = default!;
    public int SortOrder { get; private set; }
    public DateTimeOffset UploadedAt { get; private set; }

    private ModelPhoto() { }

    public static ModelPhoto Create(ModelId modelId, string fileName, string storagePath, int sortOrder)
    {
        return new ModelPhoto
        {
            Id = Guid.NewGuid(), ModelId = modelId,
            FileName = fileName, StoragePath = storagePath,
            SortOrder = sortOrder, UploadedAt = DateTimeOffset.UtcNow,
        };
    }
}

public sealed class ModelFabric
{
    public Guid Id { get; private set; }
    public ModelId ModelId { get; private set; }
    public FabricId FabricId { get; private set; }

    private ModelFabric() { }

    public static ModelFabric Create(ModelId modelId, FabricId fabricId)
        => new() { Id = Guid.NewGuid(), ModelId = modelId, FabricId = fabricId };
}
