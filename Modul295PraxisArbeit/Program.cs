using Microsoft.EntityFrameworkCore;
using Modul295PraxisArbeit.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Modul295PraxisArbeit.Services;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using Serilog;
using MongoDB.Driver;
using Modul295PraxisArbeitOrder.Data;
using Modul295PraxisArbeitOrder.Services;
using Modul295PraxisArbeitOrder.Models;


// Serilog konfigurieren
Log.Logger = new LoggerConfiguration()
    .WriteTo.File("logfile.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Console()
    .CreateLogger();

// Erstelle den WebApplication-Builder
var builder = WebApplication.CreateBuilder(args);

// Add this line to register HttpClient
builder.Services.AddHttpClient();

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
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var config = builder.Configuration.GetSection("MongoDbSettings");
    string? connectionString = config["ConnectionString"];
    string? databaseName = config["DatabaseName"];

    if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(databaseName))
    {
        throw new InvalidOperationException("MongoDB Connection String oder Database Name fehlt in den Konfigurationsdateien.");
    }

    var client = new MongoClient(connectionString);
    return client.GetDatabase(databaseName);
});

builder.Services.AddScoped<IOrderService, OrderServiceService>(); // Stellt sicher, dass der Service registriert ist

// Serilog als Logging-Provider registrieren
builder.Host.UseSerilog();

// Fügen Sie CORS hinzu und konfigurieren Sie es
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        policy => policy.AllowAnyOrigin() // Erlaubt jede Origin
                        .AllowAnyMethod() // Erlaubt alle HTTP-Methoden
                        .AllowAnyHeader()); // Erlaubt alle Header
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

app.UseCors("AllowAllOrigins");

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

static void EnsureDatabaseAndTablesExist(string DefaultConnection)
{
    string createDatabaseScript = @"
            IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'JetStreamDB')
            BEGIN
                CREATE DATABASE JetStreamDB;
            END
            ";
    try
    {
        using (SqlConnection connection = new SqlConnection(DefaultConnection))
        {
            connection.Open();

            using (SqlCommand command = new SqlCommand(createDatabaseScript, connection))
            {
                command.ExecuteNonQuery();
            }

            /*
                                using (SqlCommand command = new SqlCommand(createTablesScript, connection))
                                {
                                    command.ExecuteNonQuery();
                                }
            */
            Console.WriteLine("Datenbank und Tabellen wurden sichergestellt.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Fehler bei der Datenbankerstellung: " + ex.Message);
    }
}

// Starte die Anwendung
app.Run();