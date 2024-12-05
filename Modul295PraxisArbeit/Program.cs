using Microsoft.EntityFrameworkCore;
using Modul295PraxisArbeit.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Modul295PraxisArbeit.Services;


// Erstelle den WebApplication-Builder
var builder = WebApplication.CreateBuilder(args);

// Geheimschlüssel für JWT (Sollte aus Konfigurationsdateien oder einem sicheren Speicher kommen)
var key = builder.Configuration["JwtSettings:Key"];

// Konfiguriere den DbContext für SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Konfiguriere Authentifikation mit JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false, // Deaktiviere Überprüfung des Herausgebers (Issuer)
            ValidateAudience = false, // Deaktiviere Überprüfung der Zielgruppe (Audience)
            ValidateLifetime = true, // Überprüfe die Gültigkeit des Tokens
            ValidateIssuerSigningKey = true, // Überprüfe die Signatur des Tokens
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)) // Setze den geheimen Schlüssel
        };
    });

// Füge den JwtService als Singleton hinzu
builder.Services.AddSingleton<IJwtService>(sp => new JwtService(key));

// Füge Controller und andere notwendige Dienste hinzu
builder.Services.AddControllers();

// Swagger/OpenAPI für API-Dokumentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Erstelle die Anwendung
var app = builder.Build();

// Konfiguriere die HTTP-Pipeline für Entwicklungsumgebung
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Aktiviere HTTPS-Umleitungen
app.UseHttpsRedirection();

// Füge Authentifikations- und Autorisierungs-Middleware hinzu
app.UseAuthentication();
app.UseAuthorization();

// Mappe die Controller
app.MapControllers();

// Starte die Anwendung
app.Run();
