using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Couture.Clients.Domain;
using Couture.Clients.Features.ManageMeasurementFields;

namespace Couture.Clients.Tests;

public class MeasurementFieldHandlerTests
{
    [Fact]
    public async Task CreateField_ValidCommand_CreatesField()
    {
        using var db = TestDbHelper.CreateInMemoryContext();
        var handler = new CreateMeasurementFieldHandler(db);

        var id = await handler.Handle(
            new CreateMeasurementFieldCommand("Tour de poitrine", "cm", 1),
            CancellationToken.None);

        id.Should().NotBeEmpty();
        var field = await db.MeasurementFields.FirstAsync();
        field.Name.Should().Be("Tour de poitrine");
        field.Unit.Should().Be("cm");
        field.DisplayOrder.Should().Be(1);
        field.IsActive.Should().BeTrue();
        field.IsDefault.Should().BeFalse();
    }

    [Fact]
    public async Task CreateField_DuplicateName_Throws()
    {
        using var db = TestDbHelper.CreateInMemoryContext();
        var handler = new CreateMeasurementFieldHandler(db);

        await handler.Handle(
            new CreateMeasurementFieldCommand("Tour de taille", "cm", 1),
            CancellationToken.None);

        var act = async () => await handler.Handle(
            new CreateMeasurementFieldCommand("Tour de taille", "cm", 2),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task CreateField_DuplicateNameEvenIfDeactivated_Throws()
    {
        using var db = TestDbHelper.CreateInMemoryContext();
        var createHandler = new CreateMeasurementFieldHandler(db);
        var deleteHandler = new DeleteMeasurementFieldHandler(db);

        var id = await createHandler.Handle(
            new CreateMeasurementFieldCommand("Épaule", "cm", 1),
            CancellationToken.None);

        await deleteHandler.Handle(
            new DeleteMeasurementFieldCommand(id),
            CancellationToken.None);

        var act = async () => await createHandler.Handle(
            new CreateMeasurementFieldCommand("Épaule", "cm", 2),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CreateField_DifferentNames_BothSucceed()
    {
        using var db = TestDbHelper.CreateInMemoryContext();
        var handler = new CreateMeasurementFieldHandler(db);

        var id1 = await handler.Handle(
            new CreateMeasurementFieldCommand("Tour de poitrine", "cm", 1),
            CancellationToken.None);
        var id2 = await handler.Handle(
            new CreateMeasurementFieldCommand("Tour de taille", "cm", 2),
            CancellationToken.None);

        id1.Should().NotBe(id2);
        (await db.MeasurementFields.CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task DeleteField_ExistingField_DeactivatesIt()
    {
        using var db = TestDbHelper.CreateInMemoryContext();
        var createHandler = new CreateMeasurementFieldHandler(db);
        var deleteHandler = new DeleteMeasurementFieldHandler(db);

        var id = await createHandler.Handle(
            new CreateMeasurementFieldCommand("Carrure dos", "cm", 5),
            CancellationToken.None);

        await deleteHandler.Handle(
            new DeleteMeasurementFieldCommand(id),
            CancellationToken.None);

        var field = await db.MeasurementFields.FirstAsync();
        field.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteField_NonExistent_Throws()
    {
        using var db = TestDbHelper.CreateInMemoryContext();
        var handler = new DeleteMeasurementFieldHandler(db);

        var act = async () => await handler.Handle(
            new DeleteMeasurementFieldCommand(Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }
}
