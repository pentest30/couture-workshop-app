using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Couture.Clients.Contracts;
using Couture.Clients.Domain;
using Couture.Clients.Features.CreateClient;
using Couture.Clients.Features.ManageMeasurementFields;
using Couture.Clients.Features.RecordMeasurements;
using Couture.Clients.Features.GetMeasurementHistory;
using Couture.Clients.Persistence;

namespace Couture.Clients.Tests;

/// <summary>
/// Tests that reproduce real user misuse scenarios (duplicate fields, bad input, etc.)
/// that caused the production crash.
/// </summary>
public class MeasurementFieldResolutionTests
{
    /// <summary>
    /// Reproduces the exact production crash: duplicate field names cause
    /// ToDictionary to throw. The fix uses GroupBy so this must not throw.
    /// </summary>
    [Fact]
    public async Task FieldResolution_DuplicateFieldNamesInDb_DoesNotCrash()
    {
        using var db = TestDbHelper.CreateInMemoryContext();

        // Manually insert two fields with the same name (simulating pre-fix data)
        var field1 = MeasurementField.Create("tour poignet", "cm", 1);
        var field2 = MeasurementField.Create("tour poignet", "cm", 2);
        db.MeasurementFields.Add(field1);
        db.MeasurementFields.Add(field2);
        await db.SaveChangesAsync();

        // This is the fixed resolution logic from the endpoint
        var act = async () =>
        {
            var fieldMap = (await db.MeasurementFields.AsNoTracking()
                .Where(f => f.IsActive)
                .ToListAsync())
                .GroupBy(f => f.Name)
                .ToDictionary(g => g.Key, g => g.First().Id.Value);

            fieldMap.Should().ContainKey("tour poignet");
        };

        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Deactivated fields should not be resolved by name.
    /// </summary>
    [Fact]
    public async Task FieldResolution_DeactivatedField_NotResolved()
    {
        using var db = TestDbHelper.CreateInMemoryContext();

        var fieldId = await new CreateMeasurementFieldHandler(db).Handle(
            new CreateMeasurementFieldCommand("Épaule", "cm", 1), CancellationToken.None);
        await new DeleteMeasurementFieldHandler(db).Handle(
            new DeleteMeasurementFieldCommand(fieldId), CancellationToken.None);

        var fieldMap = (await db.MeasurementFields.AsNoTracking()
            .Where(f => f.IsActive)
            .ToListAsync())
            .GroupBy(f => f.Name)
            .ToDictionary(g => g.Key, g => g.First().Id.Value);

        fieldMap.Should().BeEmpty();
    }

    /// <summary>
    /// When active and inactive fields share a name, only the active one is resolved.
    /// </summary>
    [Fact]
    public async Task FieldResolution_ActiveAndInactiveSameName_ResolvesToActive()
    {
        using var db = TestDbHelper.CreateInMemoryContext();

        // Insert inactive field manually
        var inactive = MeasurementField.Create("tour poignet", "cm", 1);
        inactive.Deactivate();
        db.MeasurementFields.Add(inactive);

        // Insert active field with same name
        var active = MeasurementField.Create("tour poignet", "cm", 2);
        db.MeasurementFields.Add(active);
        await db.SaveChangesAsync();

        var fieldMap = (await db.MeasurementFields.AsNoTracking()
            .Where(f => f.IsActive)
            .ToListAsync())
            .GroupBy(f => f.Name)
            .ToDictionary(g => g.Key, g => g.First().Id.Value);

        fieldMap["tour poignet"].Should().Be(active.Id.Value);
    }

    /// <summary>
    /// Unknown field name in the request should be silently skipped, not crash.
    /// </summary>
    [Fact]
    public async Task FieldResolution_UnknownFieldName_SkippedSilently()
    {
        using var db = TestDbHelper.CreateInMemoryContext();

        await new CreateMeasurementFieldHandler(db).Handle(
            new CreateMeasurementFieldCommand("Tour de poitrine", "cm", 1), CancellationToken.None);

        var fieldMap = (await db.MeasurementFields.AsNoTracking()
            .Where(f => f.IsActive)
            .ToListAsync())
            .GroupBy(f => f.Name)
            .ToDictionary(g => g.Key, g => g.First().Id.Value);

        // Simulate the endpoint logic: unknown name is simply not in the map
        fieldMap.TryGetValue("champ inconnu", out _).Should().BeFalse();
        fieldMap.Should().ContainKey("Tour de poitrine");
    }

    /// <summary>
    /// Empty fieldId (Guid.Empty) + empty fieldName should produce no entry.
    /// </summary>
    [Fact]
    public async Task FieldResolution_EmptyIdAndEmptyName_ProducesNoEntry()
    {
        using var db = TestDbHelper.CreateInMemoryContext();

        var fieldMap = (await db.MeasurementFields.AsNoTracking()
            .Where(f => f.IsActive)
            .ToListAsync())
            .GroupBy(f => f.Name)
            .ToDictionary(g => g.Key, g => g.First().Id.Value);

        // Simulate endpoint logic
        var entries = new List<MeasurementEntry>();
        var fieldId = Guid.Empty;
        string? fieldName = null;
        decimal value = 90m;

        if (fieldId != Guid.Empty)
            entries.Add(new MeasurementEntry(fieldId, value));
        else if (!string.IsNullOrEmpty(fieldName) && fieldMap.TryGetValue(fieldName, out var resolvedId))
            entries.Add(new MeasurementEntry(resolvedId, value));

        entries.Should().BeEmpty();
    }

    /// <summary>
    /// Recording the same field twice in one request should save both measurements.
    /// </summary>
    [Fact]
    public async Task RecordMeasurements_SameFieldTwiceInOneRequest_SavesBoth()
    {
        using var db = TestDbHelper.CreateInMemoryContext();
        var clientResult = await new CreateClientHandler(db).Handle(
            new CreateClientCommand("Sara", "Benali", "0550123456", null, null, null, null),
            CancellationToken.None);
        var fieldId = await new CreateMeasurementFieldHandler(db).Handle(
            new CreateMeasurementFieldCommand("Tour de poitrine", "cm", 1), CancellationToken.None);

        var handler = new RecordMeasurementsHandler(db);
        await handler.Handle(
            new RecordMeasurementsCommand(clientResult.Id,
            [
                new MeasurementEntry(fieldId, 90m),
                new MeasurementEntry(fieldId, 92m),
            ], Guid.NewGuid()),
            CancellationToken.None);

        (await db.ClientMeasurements.CountAsync()).Should().Be(2);
    }

    /// <summary>
    /// History should work correctly even when there are many updates to the same field.
    /// </summary>
    [Fact]
    public async Task GetHistory_ManyUpdatesToSameField_TracksAllChanges()
    {
        using var db = TestDbHelper.CreateInMemoryContext();
        var clientResult = await new CreateClientHandler(db).Handle(
            new CreateClientCommand("Sara", "Benali", "0550123456", null, null, null, null),
            CancellationToken.None);
        var fieldId = await new CreateMeasurementFieldHandler(db).Handle(
            new CreateMeasurementFieldCommand("Tour de taille", "cm", 1), CancellationToken.None);

        var recorder = new RecordMeasurementsHandler(db);
        var values = new[] { 68m, 70m, 71.5m, 69m };
        foreach (var v in values)
        {
            await recorder.Handle(
                new RecordMeasurementsCommand(clientResult.Id, [new MeasurementEntry(fieldId, v)], Guid.NewGuid()),
                CancellationToken.None);
        }

        var result = await new GetMeasurementHistoryHandler(db).Handle(
            new GetMeasurementHistoryQuery(clientResult.Id), CancellationToken.None);

        result.Current.Should().HaveCount(1);
        result.Current[0].Value.Should().Be(69m); // latest
        result.History.Should().HaveCount(3); // 3 transitions: 68→70, 70→71.5, 71.5→69
    }

    /// <summary>
    /// Measurements for one client should not appear in another client's history.
    /// </summary>
    [Fact]
    public async Task GetHistory_DifferentClients_IsolatedResults()
    {
        using var db = TestDbHelper.CreateInMemoryContext();
        var client1 = await new CreateClientHandler(db).Handle(
            new CreateClientCommand("Sara", "Benali", "0550111111", null, null, null, null),
            CancellationToken.None);
        var client2 = await new CreateClientHandler(db).Handle(
            new CreateClientCommand("Nadia", "Hamidi", "0550222222", null, null, null, null),
            CancellationToken.None);
        var fieldId = await new CreateMeasurementFieldHandler(db).Handle(
            new CreateMeasurementFieldCommand("Tour de poitrine", "cm", 1), CancellationToken.None);

        var recorder = new RecordMeasurementsHandler(db);
        await recorder.Handle(
            new RecordMeasurementsCommand(client1.Id, [new MeasurementEntry(fieldId, 90m)], Guid.NewGuid()),
            CancellationToken.None);
        await recorder.Handle(
            new RecordMeasurementsCommand(client2.Id, [new MeasurementEntry(fieldId, 85m)], Guid.NewGuid()),
            CancellationToken.None);

        var history1 = await new GetMeasurementHistoryHandler(db).Handle(
            new GetMeasurementHistoryQuery(client1.Id), CancellationToken.None);
        var history2 = await new GetMeasurementHistoryHandler(db).Handle(
            new GetMeasurementHistoryQuery(client2.Id), CancellationToken.None);

        history1.Current.Should().HaveCount(1);
        history1.Current[0].Value.Should().Be(90m);
        history2.Current.Should().HaveCount(1);
        history2.Current[0].Value.Should().Be(85m);
    }
}
