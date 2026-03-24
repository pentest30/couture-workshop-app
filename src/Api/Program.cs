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

// Auto-migrate and seed in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    // Migrate all DbContexts
    await services.GetRequiredService<Couture.Identity.Persistence.IdentityDbContext>().Database.MigrateAsync();
    await services.GetRequiredService<OrdersDbContext>().Database.EnsureCreatedAsync();
    await services.GetRequiredService<ClientsDbContext>().Database.EnsureCreatedAsync();
    await services.GetRequiredService<FinanceDbContext>().Database.EnsureCreatedAsync();
    await services.GetRequiredService<DashboardDbContext>().Database.EnsureCreatedAsync();
    await services.GetRequiredService<NotificationsDbContext>().Database.EnsureCreatedAsync();

    // Seed identity
    await services.GetRequiredService<IdentitySeeder>().SeedAsync();
}

app.Run();
