using Xunit;
using FluentAssertions;
using Couture.Clients.Features.CreateClient;
using Couture.Clients.Features.ManageMeasurementFields;
using Couture.Clients.Features.RecordMeasurements;
using Couture.Clients.Features.GetMeasurementHistory;

namespace Couture.Clients.Tests;

public class GetMeasurementHistoryHandlerTests
{
    private static async Task<(Guid ClientId, Guid FieldId)> SeedClientAndField(
        Persistence.ClientsDbContext db)
    {
        var clientResult = await new CreateClientHandler(db).Handle(
            new CreateClientCommand("Sara", "Benali", "0550123456", null, null, null, null),
            CancellationToken.None);

        var fieldId = await new CreateMeasurementFieldHandler(db).Handle(
            new CreateMeasurementFieldCommand("Tour de poitrine", "cm", 1),
            CancellationToken.None);

        return (clientResult.Id, fieldId);
    }

    [Fact]
    public async Task GetHistory_NoMeasurements_ReturnsEmpty()
    {
        using var db = TestDbHelper.CreateInMemoryContext();
        var (clientId, _) = await SeedClientAndField(db);
        var handler = new GetMeasurementHistoryHandler(db);

        var result = await handler.Handle(
            new GetMeasurementHistoryQuery(clientId),
            CancellationToken.None);

        result.Current.Should().BeEmpty();
        result.History.Should().BeEmpty();
    }

    [Fact]
    public async Task GetHistory_SingleMeasurement_ReturnsCurrent()
    {
        using var db = TestDbHelper.CreateInMemoryContext();
        var (clientId, fieldId) = await SeedClientAndField(db);

        await new RecordMeasurementsHandler(db).Handle(
            new RecordMeasurementsCommand(clientId, [new MeasurementEntry(fieldId, 92.5m)], Guid.NewGuid()),
            CancellationToken.None);

        var handler = new GetMeasurementHistoryHandler(db);
        var result = await handler.Handle(new GetMeasurementHistoryQuery(clientId), CancellationToken.None);

        result.Current.Should().HaveCount(1);
        result.Current[0].FieldName.Should().Be("Tour de poitrine");
        result.Current[0].Value.Should().Be(92.5m);
        result.History.Should().BeEmpty();
    }

    [Fact]
    public async Task GetHistory_TwoMeasurementsSameField_ReturnsCurrentAndHistory()
    {
        using var db = TestDbHelper.CreateInMemoryContext();
        var (clientId, fieldId) = await SeedClientAndField(db);
        var recorder = new RecordMeasurementsHandler(db);

        await recorder.Handle(
            new RecordMeasurementsCommand(clientId, [new MeasurementEntry(fieldId, 90m)], Guid.NewGuid()),
            CancellationToken.None);

        await recorder.Handle(
            new RecordMeasurementsCommand(clientId, [new MeasurementEntry(fieldId, 92m)], Guid.NewGuid()),
            CancellationToken.None);

        var result = await new GetMeasurementHistoryHandler(db).Handle(
            new GetMeasurementHistoryQuery(clientId), CancellationToken.None);

        result.Current.Should().HaveCount(1);
        result.Current[0].Value.Should().Be(92m);
        result.History.Should().HaveCount(1);
        result.History[0].OldValue.Should().Be(90m);
        result.History[0].NewValue.Should().Be(92m);
    }

    [Fact]
    public async Task GetHistory_MultipleFields_ReturnsCurrentForEach()
    {
        using var db = TestDbHelper.CreateInMemoryContext();
        var clientResult = await new CreateClientHandler(db).Handle(
            new CreateClientCommand("Sara", "Benali", "0550123456", null, null, null, null),
            CancellationToken.None);
        var fieldHandler = new CreateMeasurementFieldHandler(db);
        var field1 = await fieldHandler.Handle(new CreateMeasurementFieldCommand("Tour de poitrine", "cm", 1), CancellationToken.None);
        var field2 = await fieldHandler.Handle(new CreateMeasurementFieldCommand("Tour de taille", "cm", 2), CancellationToken.None);

        await new RecordMeasurementsHandler(db).Handle(
            new RecordMeasurementsCommand(clientResult.Id,
            [
                new MeasurementEntry(field1, 92m),
                new MeasurementEntry(field2, 70m),
            ], Guid.NewGuid()),
            CancellationToken.None);

        var result = await new GetMeasurementHistoryHandler(db).Handle(
            new GetMeasurementHistoryQuery(clientResult.Id), CancellationToken.None);

        result.Current.Should().HaveCount(2);
        result.Current.Should().Contain(m => m.FieldName == "Tour de poitrine" && m.Value == 92m);
        result.Current.Should().Contain(m => m.FieldName == "Tour de taille" && m.Value == 70m);
    }

    [Fact]
    public async Task GetHistory_DeletedFieldMeasurements_ExcludedFromResults()
    {
        using var db = TestDbHelper.CreateInMemoryContext();
        var (clientId, fieldId) = await SeedClientAndField(db);

        await new RecordMeasurementsHandler(db).Handle(
            new RecordMeasurementsCommand(clientId, [new MeasurementEntry(fieldId, 92m)], Guid.NewGuid()),
            CancellationToken.None);

        // Remove the field from the DB entirely (simulating orphaned data)
        var field = await db.MeasurementFields.FindAsync(Contracts.MeasurementFieldId.From(fieldId));
        db.MeasurementFields.Remove(field!);
        await db.SaveChangesAsync();

        var result = await new GetMeasurementHistoryHandler(db).Handle(
            new GetMeasurementHistoryQuery(clientId), CancellationToken.None);

        result.Current.Should().BeEmpty();
    }
}
