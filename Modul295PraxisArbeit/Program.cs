using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using MongoDB.Driver;
using Modul295PraxisArbeit.Services;
using Modul295PraxisArbeit.Data;
using Modul295PraxisArbeit.Services;
using Modul295PraxisArbeit.Models;

// 📌 Define the log file path
var logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "application.log");

// 📌 Ensure the Logs directory exists
string? logsFolder = Path.GetDirectoryName(logFilePath);
if (!string.IsNullOrEmpty(logsFolder))
{
    Directory.CreateDirectory(logsFolder);
}

// 📌 Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Infinite, retainedFileCountLimit: 1)
    .CreateLogger();

// 📌 Create WebApplication Builder
var builder = WebApplication.CreateBuilder(args);

// 🔹 Add HttpClient
builder.Services.AddHttpClient();

// 🔹 Register Background Test Runner
builder.Services.AddHostedService<TestRunnerService>();

// 🔹 Load JWT Secret Key
var jwtKey = builder.Configuration["JwtSettings:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new Exception("JWT Secret Key is missing. Add it in appsettings.json.");
}

// 🔹 Configure Authentication with JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // 🔹 Allow HTTP during local development
        options.SaveToken = true; // 🔹 Save token in the context
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true, // 🔥 Ensures token expires correctly
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero // ⏰ Disables the default 5-minute grace period for expired tokens
        };

        // 🔍 Add error handling for invalid tokens
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"❌ JWT Authentication Failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                var errorMessage = new { message = "❌ Unauthorized: Invalid or expired token" };
                return context.Response.WriteAsJsonAsync(errorMessage);
            }
        };
    });


// 🔹 Configure MongoDB
var mongoConfig = builder.Configuration.GetSection("MongoDbSettings");
string? mongoConnectionString = mongoConfig["ConnectionString"];
string? mongoDatabaseName = mongoConfig["DatabaseName"];

if (string.IsNullOrEmpty(mongoConnectionString) || string.IsNullOrEmpty(mongoDatabaseName))
{
    throw new InvalidOperationException("MongoDB Connection String or Database Name is missing in the configuration files.");
}

var mongoClient = new MongoClient(mongoConnectionString);
var mongoDatabase = mongoClient.GetDatabase(mongoDatabaseName);

builder.Services.AddSingleton<IMongoDatabase>(mongoDatabase);

// 🔹 Register OrderService
builder.Services.AddScoped<IOrderService, OrderServiceService>();

// 🔹 Register JwtService
builder.Services.AddSingleton<IJwtService>(sp => new JwtService(jwtKey));

// 🔹 Register Serilog
builder.Host.UseSerilog();

// 🔹 Configure CORS (Ensure it allows frontend)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("http://localhost:5173") // ✅ Your React App URL
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});



AppContext.SetSwitch("System.Drawing.EnableUnixSupport", true);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // ✅ Keep JSON property names as-is
    });


// 🔹 Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 📌 Build the Application
var app = builder.Build();

// 🔹 Enable CORS
app.UseCors("AllowFrontend");

// 🔹 Enable Swagger in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 🔹 Enable HTTPS Redirection
app.UseHttpsRedirection();

// 🔹 Enable Logging Middleware
app.UseSerilogRequestLogging();

// 🔹 Enable Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// 🔹 Map Controllers
app.MapControllers();

// 📌 Run the Application
app.Run();
