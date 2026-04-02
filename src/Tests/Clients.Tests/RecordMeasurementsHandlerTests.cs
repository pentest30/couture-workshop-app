using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Couture.Clients.Domain;
using Couture.Clients.Features.CreateClient;
using Couture.Clients.Features.ManageMeasurementFields;
using Couture.Clients.Features.RecordMeasurements;

namespace Couture.Clients.Tests;

public class RecordMeasurementsHandlerTests
{
    private static async Task<(Guid ClientId, Guid FieldId)> SeedClientAndField(
        Persistence.ClientsDbContext db, string fieldName = "Tour de poitrine")
    {
        var clientHandler = new CreateClientHandler(db);
        var clientResult = await clientHandler.Handle(
            new CreateClientCommand("Sara", "Benali", "0550123456", null, null, null, null),
            CancellationToken.None);

        var fieldHandler = new CreateMeasurementFieldHandler(db);
        var fieldId = await fieldHandler.Handle(
            new CreateMeasurementFieldCommand(fieldName, "cm", 1),
            CancellationToken.None);

        return (clientResult.Id, fieldId);
    }

    [Fact]
    public async Task RecordMeasurements_ValidEntry_SavesMeasurement()
    {
        using var db = TestDbHelper.CreateInMemoryContext();
        var (clientId, fieldId) = await SeedClientAndField(db);
        var handler = new RecordMeasurementsHandler(db);
        var userId = Guid.NewGuid();

        await handler.Handle(
            new RecordMeasurementsCommand(clientId, [new MeasurementEntry(fieldId, 92.5m)], userId),
            CancellationToken.None);

        var measurement = await db.ClientMeasurements.FirstAsync();
        measurement.Value.Should().Be(92.5m);
        measurement.MeasuredBy.Should().Be(userId);
    }

    [Fact]
    public async Task RecordMeasurements_MultipleEntries_SavesAll()
    {
        using var db = TestDbHelper.CreateInMemoryContext();
        var clientHandler = new CreateClientHandler(db);
        var clientResult = await clientHandler.Handle(
            new CreateClientCommand("Sara", "Benali", "0550123456", null, null, null, null),
            CancellationToken.None);

        var fieldHandler = new CreateMeasurementFieldHandler(db);
        var field1 = await fieldHandler.Handle(new CreateMeasurementFieldCommand("Tour de poitrine", "cm", 1), CancellationToken.None);
        var field2 = await fieldHandler.Handle(new CreateMeasurementFieldCommand("Tour de taille", "cm", 2), CancellationToken.None);

        var handler = new RecordMeasurementsHandler(db);
        await handler.Handle(
            new RecordMeasurementsCommand(clientResult.Id,
            [
                new MeasurementEntry(field1, 92.5m),
                new MeasurementEntry(field2, 70.0m),
            ], Guid.NewGuid()),
            CancellationToken.None);

        (await db.ClientMeasurements.CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task RecordMeasurements_NonExistentClient_Throws()
    {
        using var db = TestDbHelper.CreateInMemoryContext();
        var handler = new RecordMeasurementsHandler(db);

        var act = async () => await handler.Handle(
            new RecordMeasurementsCommand(Guid.NewGuid(), [new MeasurementEntry(Guid.NewGuid(), 80m)], Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task RecordMeasurements_ZeroValue_Throws()
    {
        using var db = TestDbHelper.CreateInMemoryContext();
        var (clientId, fieldId) = await SeedClientAndField(db);
        var handler = new RecordMeasurementsHandler(db);

        var act = async () => await handler.Handle(
            new RecordMeasurementsCommand(clientId, [new MeasurementEntry(fieldId, 0m)], Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task RecordMeasurements_NegativeValue_Throws()
    {
        using var db = TestDbHelper.CreateInMemoryContext();
        var (clientId, fieldId) = await SeedClientAndField(db);
        var handler = new RecordMeasurementsHandler(db);

        var act = async () => await handler.Handle(
            new RecordMeasurementsCommand(clientId, [new MeasurementEntry(fieldId, -5m)], Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}
