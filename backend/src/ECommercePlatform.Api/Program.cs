using System.Text;
using ECommercePlatform.BuildingBlocks.Application;
using ECommercePlatform.BuildingBlocks.Infrastructure;
using ECommercePlatform.BuildingBlocks.Infrastructure.Security;
using ECommercePlatform.BuildingBlocks.Messaging;
using ECommercePlatform.Modules.Cart.Infrastructure;
using ECommercePlatform.Modules.Cart.Infrastructure.Persistence;
using ECommercePlatform.Modules.Catalog.Infrastructure;
using ECommercePlatform.Modules.Catalog.Infrastructure.Persistence;
using ECommercePlatform.Modules.Identity.Infrastructure;
using ECommercePlatform.Modules.Identity.Infrastructure.Persistence;
using ECommercePlatform.Modules.Inventory.Infrastructure;
using ECommercePlatform.Modules.Inventory.Infrastructure.Persistence;
using ECommercePlatform.Modules.Order.Infrastructure;
using ECommercePlatform.Modules.Order.Infrastructure.Persistence;
using ECommercePlatform.Modules.Promotion.Infrastructure;
using ECommercePlatform.Modules.Promotion.Infrastructure.Persistence;
using ECommercePlatform.Modules.Review.Infrastructure;
using ECommercePlatform.Modules.Review.Infrastructure.Persistence;
using ECommercePlatform.Modules.Shipping.Infrastructure;
using ECommercePlatform.Modules.Shipping.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddApplicationBehaviors();

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("'Default' connection string is not configured.");
builder.Services.AddSqlConnectionFactory(connectionString);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

builder.Services.AddRabbitMqMessaging(builder.Configuration);

// Modules — walking skeleton: only Identity is fully implemented, the rest register an empty
// DbContext + module-scoped MediatR/FluentValidation pipeline so the DI graph is proven end to end.
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddCatalogModule(builder.Configuration);
builder.Services.AddInventoryModule(builder.Configuration);
builder.Services.AddCartModule(builder.Configuration);
builder.Services.AddOrderModule(builder.Configuration);
builder.Services.AddShippingModule(builder.Configuration);
builder.Services.AddPromotionModule(builder.Configuration);
builder.Services.AddReviewModule(builder.Configuration);

var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var jwtIssuer = jwtSection["Issuer"]!;
var jwtAudience = jwtSection["Audience"]!;
var jwtSigningKey = jwtSection["SigningKey"]!;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Keep claim types exactly as issued ("sub" stays "sub") instead of the legacy
        // ClaimTypes.NameIdentifier remapping, so controllers can read JwtRegisteredClaimNames.Sub directly.
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });

builder.Services.AddAuthorization();

// Faz 5: the frontend (Next.js dev server, arbitrary localhost port) is a different origin from
// this API. In Development any loopback origin is allowed — the frontend's port varies (fixed
// 3000 normally, but the preview tooling used during development reassigns it when busy).
// Non-Development environments use an explicit allowlist from config instead.
const string frontendCorsPolicy = "Frontend";
var corsAllowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy(frontendCorsPolicy, policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.SetIsOriginAllowed(origin => Uri.TryCreate(origin, UriKind.Absolute, out var uri) && uri.IsLoopback);
        }
        else
        {
            policy.WithOrigins(corsAllowedOrigins);
        }

        policy.AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors(frontendCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Every module owns its own DbContext/schema (bkz. CLAUDE.md §1), so each one needs its own
// migration call — this makes `docker compose up` against a fresh, empty Postgres volume produce
// a fully-migrated database without a separate manual `dotnet ef database update` step per module
// (mirrors WMS's WMS.Api/Program.cs).
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await services.GetRequiredService<IdentityDbContext>().Database.MigrateAsync();
    await services.GetRequiredService<CatalogDbContext>().Database.MigrateAsync();
    await services.GetRequiredService<InventoryDbContext>().Database.MigrateAsync();
    await services.GetRequiredService<CartDbContext>().Database.MigrateAsync();
    await services.GetRequiredService<OrderDbContext>().Database.MigrateAsync();
    await services.GetRequiredService<ShippingDbContext>().Database.MigrateAsync();
    await services.GetRequiredService<PromotionDbContext>().Database.MigrateAsync();
    await services.GetRequiredService<ReviewDbContext>().Database.MigrateAsync();
}

app.Run();

public partial class Program;
