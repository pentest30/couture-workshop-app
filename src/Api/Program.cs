using Couture.Api.Endpoints;
using Couture.Identity;
using Couture.Identity.Services;
using Couture.Orders.Persistence;
using Couture.Clients.Persistence;
using Couture.Finance.Persistence;
using Couture.Dashboard.Persistence;
using Couture.Notifications.Persistence;
using Couture.Catalog.Persistence;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using Scalar.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File("logs/couture-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
    .CreateLogger();

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

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

builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseNpgsql(connectionString));

// Mediator (source-generated CQRS)
builder.Services.AddMediator(options =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
});

// Notifications services
builder.Services.AddScoped<Couture.Notifications.Domain.IManagerResolver, Couture.Api.Services.ManagerResolver>();
builder.Services.AddScoped<Couture.Notifications.Domain.NotificationService>();
builder.Services.AddScoped<Couture.Notifications.Sms.ISmsGateway, Couture.Notifications.Sms.MockSmsGateway>();
builder.Services.AddScoped<Couture.Notifications.Jobs.EvaluateOverdueOrdersJob>();
builder.Services.AddScoped<Couture.Notifications.Jobs.EvaluateStalledOrdersJob>();
builder.Services.AddScoped<Couture.Notifications.Jobs.PurgeExpiredNotificationsJob>();
builder.Services.AddHostedService<Couture.Notifications.Jobs.NotificationJobScheduler>();
builder.Services.AddSignalR();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

// OpenAPI + Scalar
app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.WithTitle("Couture Workshop API");
});

// Static files for uploads
app.UseStaticFiles();

// Middleware
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<Couture.Notifications.Hub.NotificationHub>("/hubs/notifications");

// Map module endpoints
app.MapAuthEndpoints();
app.MapOrderEndpoints();
app.MapClientEndpoints();
app.MapFinanceEndpoints();
app.MapDashboardEndpoints();
app.MapNotificationEndpoints();
app.MapCatalogEndpoints();
app.MapUploadEndpoints();
app.MapUserEndpoints();

// Auto-migrate and seed
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
    await services.GetRequiredService<CatalogDbContext>().Database.MigrateAsync();

    // Seed identity (roles + admin user) in all environments
    await services.GetRequiredService<IdentitySeeder>().SeedAsync();

    // Seed measurement fields in all environments (required for measurements to work)
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

    // Seed notification configs in all environments
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

    if (app.Environment.IsDevelopment())
    {
        logger.LogInformation("Seeding sample data...");
        await SeedSampleDataAsync(services, logger);
    }

    logger.LogInformation("Startup complete.");
}

app.Run();

static async Task SeedSampleDataAsync(IServiceProvider services, Microsoft.Extensions.Logging.ILogger logger)
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

    // Seed catalog
    var catalogDb = services.GetRequiredService<CatalogDbContext>();
    if (!await catalogDb.Models.AnyAsync())
    {
        var fabrics = new[]
        {
            Couture.Catalog.Domain.Fabric.Create("Satin duchesse", "Satin", "#1a237e", 3500m, 25, "Tissu Benali", "Satin lourd pour robes de cérémonie"),
            Couture.Catalog.Domain.Fabric.Create("Velours bordeaux", "Velours", "#7f0000", 4200m, 18, "Tissu Benali", "Velours épais pour caftans"),
            Couture.Catalog.Domain.Fabric.Create("Mousseline ivoire", "Mousseline", "#fffde7", 2800m, 30, "Tissu El Djazair", "Mousseline légère pour voiles et superpositions"),
            Couture.Catalog.Domain.Fabric.Create("Brocart doré", "Brocart", "#f9a825", 5500m, 12, "Import Istanbul", "Brocart à motifs floraux dorés"),
            Couture.Catalog.Domain.Fabric.Create("Organza champagne", "Organza", "#fff8e1", 3000m, 20, "Tissu El Djazair", "Organza transparent pour superpositions"),
            Couture.Catalog.Domain.Fabric.Create("Taffetas émeraude", "Taffetas", "#1b5e20", 3200m, 15, "Tissu Benali"),
            Couture.Catalog.Domain.Fabric.Create("Dentelle française", "Dentelle", "#f5f5f5", 6000m, 8, "Import Paris", "Dentelle de Calais pour finitions"),
            Couture.Catalog.Domain.Fabric.Create("Crêpe de Chine", "Crêpe", "#e0e0e0", 2500m, 22, "Tissu El Djazair"),
        };
        catalogDb.Fabrics.AddRange(fabrics);
        await catalogDb.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} fabrics", fabrics.Length);

        var models = new[]
        {
            Couture.Catalog.Domain.Model.Create("MOD-2026-0001", "Caftan Cérémonie Classique",
                Couture.Catalog.Domain.ModelCategory.Ceremonie, "Brode", 25000m, 14, true,
                "Caftan brodé traditionnel pour cérémonies. Broderie au fil doré sur velours, manches évasées, ceinture intégrée."),
            Couture.Catalog.Domain.Model.Create("MOD-2026-0002", "Karakou Mariée",
                Couture.Catalog.Domain.ModelCategory.Mariee, "Mixte", 55000m, 21, true,
                "Karakou complet pour mariée. Broderie fetla + perlage intégral. Veste + jupe + seroual. Tissu velours + brocart."),
            Couture.Catalog.Domain.Model.Create("MOD-2026-0003", "Robe de Soirée Simple",
                Couture.Catalog.Domain.ModelCategory.Ceremonie, "Simple", 12000m, 5, true,
                "Robe de soirée élégante en satin. Coupe droite, fente latérale, dos nu. Idéale pour invitées de mariage."),
            Couture.Catalog.Domain.Model.Create("MOD-2026-0004", "Blousa Oranaise Brodée",
                Couture.Catalog.Domain.ModelCategory.Traditionnel, "Brode", 35000m, 18, true,
                "Blousa traditionnelle d'Oran avec broderie dorée sur velours. Col montant, manches longues évasées."),
            Couture.Catalog.Domain.Model.Create("MOD-2026-0005", "Robe Perlée Soirée",
                Couture.Catalog.Domain.ModelCategory.Moderne, "Perle", 30000m, 12, true,
                "Robe longue perlée moderne. Perles et cristaux sur mousseline. Coupe sirène, décolleté en V."),
            Couture.Catalog.Domain.Model.Create("MOD-2026-0006", "Gandoura Quotidienne",
                Couture.Catalog.Domain.ModelCategory.Quotidien, "Simple", 8000m, 3, true,
                "Gandoura confortable pour la maison. Tissu crêpe, coupe ample, finitions soignées."),
            Couture.Catalog.Domain.Model.Create("MOD-2026-0007", "Caftan Moderne Perlé",
                Couture.Catalog.Domain.ModelCategory.Moderne, "Mixte", 42000m, 18, true,
                "Caftan contemporain avec broderie légère et perlage sur les manches et le col. Tissu organza et satin."),
            Couture.Catalog.Domain.Model.Create("MOD-2026-0008", "Jebba Fergani",
                Couture.Catalog.Domain.ModelCategory.Traditionnel, "Brode", 45000m, 20, true,
                "Jebba constantinoise traditionnelle. Broderie fetla complète, col rond fermé, tissu velours."),
        };
        // Link fabrics before saving
        models[0].LinkFabric(fabrics[1].Id);
        models[0].LinkFabric(fabrics[3].Id);
        models[1].LinkFabric(fabrics[1].Id);
        models[1].LinkFabric(fabrics[3].Id);
        models[1].LinkFabric(fabrics[6].Id);
        models[2].LinkFabric(fabrics[0].Id);
        models[3].LinkFabric(fabrics[1].Id);
        models[4].LinkFabric(fabrics[2].Id);
        models[5].LinkFabric(fabrics[7].Id);
        models[6].LinkFabric(fabrics[4].Id);
        models[6].LinkFabric(fabrics[0].Id);
        models[7].LinkFabric(fabrics[1].Id);
        models[7].LinkFabric(fabrics[3].Id);

        catalogDb.Models.AddRange(models);
        await catalogDb.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} catalog models with fabric links", models.Length);
    }
}
