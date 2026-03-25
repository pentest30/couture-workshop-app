using Couture.Api.Endpoints;
using Couture.Identity;
using Couture.Identity.Services;
using Couture.Orders.Persistence;
using Couture.Clients.Persistence;
using Couture.Finance.Persistence;
using Couture.Dashboard.Persistence;
using Couture.Notifications.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Identity module (auth, JWT, roles, permissions)
builder.Services.AddIdentityModule(builder.Configuration);

// Module DbContexts
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDbContext<ClientsDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDbContext<FinanceDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDbContext<DashboardDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDbContext<NotificationsDbContext>(options =>
    options.UseNpgsql(connectionString));

// Mediator (source-generated CQRS)
builder.Services.AddMediator(options =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// OpenAPI
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Middleware
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Map module endpoints
app.MapAuthEndpoints();
app.MapOrderEndpoints();
app.MapClientEndpoints();
app.MapFinanceEndpoints();
app.MapDashboardEndpoints();

// Auto-migrate and seed in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

    logger.LogInformation("Applying migrations...");

    await services.GetRequiredService<Couture.Identity.Persistence.IdentityDbContext>().Database.MigrateAsync();
    await services.GetRequiredService<OrdersDbContext>().Database.MigrateAsync();
    await services.GetRequiredService<ClientsDbContext>().Database.MigrateAsync();
    await services.GetRequiredService<FinanceDbContext>().Database.MigrateAsync();
    await services.GetRequiredService<DashboardDbContext>().Database.MigrateAsync();
    await services.GetRequiredService<NotificationsDbContext>().Database.MigrateAsync();

    logger.LogInformation("Seeding data...");

    // Seed identity (roles + admin user)
    await services.GetRequiredService<IdentitySeeder>().SeedAsync();

    // Seed sample data
    await SeedSampleDataAsync(services, logger);

    logger.LogInformation("Startup complete.");
}

app.Run();

static async Task SeedSampleDataAsync(IServiceProvider services, ILogger logger)
{
    // Seed measurement fields
    var clientsDb = services.GetRequiredService<ClientsDbContext>();
    if (!await clientsDb.MeasurementFields.AnyAsync())
    {
        var fields = new[]
        {
            ("Tour de poitrine", "cm", 1),
            ("Tour de taille", "cm", 2),
            ("Tour de hanches", "cm", 3),
            ("Longueur robe (dos)", "cm", 4),
            ("Longueur jupe", "cm", 5),
            ("Longueur manche", "cm", 6),
            ("Tour de bras", "cm", 7),
            ("Épaule", "cm", 8),
            ("Carrure dos", "cm", 9),
            ("Hauteur totale", "cm", 10),
        };
        foreach (var (name, unit, order) in fields)
        {
            clientsDb.MeasurementFields.Add(
                Couture.Clients.Domain.MeasurementField.Create(name, unit, order, isDefault: true));
        }
        await clientsDb.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} measurement fields", fields.Length);
    }

    // Seed sample clients
    if (!await clientsDb.Clients.AnyAsync())
    {
        var clients = new[]
        {
            ("C-0001", "Sara", "Benali", "0550123456"),
            ("C-0002", "Nadia", "Hamidi", "0661234567"),
            ("C-0003", "Fatima", "Kaci", "0770345678"),
            ("C-0004", "Amira", "Boudiaf", "0550987654"),
            ("C-0005", "Yasmine", "Merabet", "0662345678"),
        };
        foreach (var (code, first, last, phone) in clients)
        {
            clientsDb.Clients.Add(
                Couture.Clients.Domain.Client.Create(code, first, last, phone));
        }
        await clientsDb.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} sample clients", clients.Length);
    }

    // Seed workshop settings
    var dashboardDb = services.GetRequiredService<DashboardDbContext>();
    if (!await dashboardDb.WorkshopSettings.AnyAsync())
    {
        dashboardDb.WorkshopSettings.Add(
            Couture.Dashboard.Domain.WorkshopSettings.CreateDefault());
        await dashboardDb.SaveChangesAsync();
        logger.LogInformation("Seeded workshop settings");
    }

    // Seed Algerian holidays
    if (!await dashboardDb.Holidays.AnyAsync())
    {
        var holidays = new[]
        {
            (new DateOnly(2026, 1, 1), "Nouvel An", true),
            (new DateOnly(2026, 5, 1), "Fête du Travail", true),
            (new DateOnly(2026, 7, 5), "Fête de l'Indépendance", true),
            (new DateOnly(2026, 11, 1), "Fête de la Révolution", true),
        };
        foreach (var (date, name, recurring) in holidays)
        {
            dashboardDb.Holidays.Add(
                Couture.Dashboard.Domain.Holiday.Create(date, name, recurring));
        }
        await dashboardDb.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} holidays", holidays.Length);
    }

    // Seed notification configs
    var notifDb = services.GetRequiredService<NotificationsDbContext>();
    if (!await notifDb.NotificationConfigs.AnyAsync())
    {
        foreach (var type in Couture.Notifications.Domain.NotificationType.List)
        {
            notifDb.NotificationConfigs.Add(
                Couture.Notifications.Domain.NotificationConfig.Create(type));
        }
        await notifDb.SaveChangesAsync();
        logger.LogInformation("Seeded notification configs for {Count} types",
            Couture.Notifications.Domain.NotificationType.List.Count);
    }

    // Seed sample orders
    var ordersDb = services.GetRequiredService<OrdersDbContext>();
    if (!await ordersDb.Orders.AnyAsync())
    {
        var client1 = await clientsDb.Clients.FirstAsync(c => c.Code == "C-0001");
        var client2 = await clientsDb.Clients.FirstAsync(c => c.Code == "C-0002");
        var client3 = await clientsDb.Clients.FirstAsync(c => c.Code == "C-0003");

        var orders = new[]
        {
            Couture.Orders.Domain.Order.Create("CMD-2026-0001", client1.Id.Value, Couture.Orders.Domain.WorkType.Simple,
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), 12000m,
                description: "Robe de soirée simple", fabric: "Satin bleu nuit"),
            Couture.Orders.Domain.Order.Create("CMD-2026-0002", client2.Id.Value, Couture.Orders.Domain.WorkType.Brode,
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)), 25000m,
                description: "Caftan brodé pour mariage", fabric: "Velours bordeaux"),
            Couture.Orders.Domain.Order.Create("CMD-2026-0003", client3.Id.Value, Couture.Orders.Domain.WorkType.Mixte,
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)), 45000m,
                description: "Karakou brodé et perlé", fabric: "Velours vert émeraude"),
            Couture.Orders.Domain.Order.Create("CMD-2026-0004", client1.Id.Value, Couture.Orders.Domain.WorkType.Perle,
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)), 18000m,
                description: "Robe perlée soirée", fabric: "Mousseline blanche"),
        };

        foreach (var order in orders)
        {
            ordersDb.Orders.Add(order);
        }
        await ordersDb.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} sample orders", orders.Length);
    }
}
