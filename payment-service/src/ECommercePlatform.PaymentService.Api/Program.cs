using System.Text;
using ECommercePlatform.BuildingBlocks.Application;
using ECommercePlatform.BuildingBlocks.Infrastructure;
using ECommercePlatform.BuildingBlocks.Infrastructure.Security;
using ECommercePlatform.BuildingBlocks.Messaging;
using ECommercePlatform.Modules.Payment.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddApplicationBehaviors();

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("'Default' connection string is not configured.");
builder.Services.AddSqlConnectionFactory(connectionString);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

builder.Services.AddRabbitMqMessaging(builder.Configuration);

builder.Services.AddPaymentModule(builder.Configuration);

// Payment.Service validates the exact same JWT (same Issuer/Audience/SigningKey config) the monolith
// issues — it never calls Identity to check a token. This is precisely the scenario CLAUDE.md madde 8
// named when JWT was chosen over cookie-sessions: "birden fazla client/servis tarafından tüketilmesi
// bekleniyor". Stateless auth means a second service just needs the same shared secret, nothing more.
var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var jwtIssuer = jwtSection["Issuer"]!;
var jwtAudience = jwtSection["Audience"]!;
var jwtSigningKey = jwtSection["SigningKey"]!;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
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
