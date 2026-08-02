using System.Text;
using ECommercePlatform.BuildingBlocks.Application;
using ECommercePlatform.BuildingBlocks.Infrastructure;
using ECommercePlatform.BuildingBlocks.Infrastructure.Security;
using ECommercePlatform.Modules.Cart.Infrastructure;
using ECommercePlatform.Modules.Catalog.Infrastructure;
using ECommercePlatform.Modules.Identity.Infrastructure;
using ECommercePlatform.Modules.Inventory.Infrastructure;
using ECommercePlatform.Modules.Order.Infrastructure;
using ECommercePlatform.Modules.Payment.Infrastructure;
using ECommercePlatform.Modules.Promotion.Infrastructure;
using ECommercePlatform.Modules.Review.Infrastructure;
using ECommercePlatform.Modules.Shipping.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

// Modules — walking skeleton: only Identity is fully implemented, the rest register an empty
// DbContext + module-scoped MediatR/FluentValidation pipeline so the DI graph is proven end to end.
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddCatalogModule(builder.Configuration);
builder.Services.AddInventoryModule(builder.Configuration);
builder.Services.AddCartModule(builder.Configuration);
builder.Services.AddOrderModule(builder.Configuration);
builder.Services.AddPaymentModule(builder.Configuration);
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;
