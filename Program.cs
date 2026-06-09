using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using RestaurantBackend.Data;
using RestaurantBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// Cloud (Render) PORT env var pe suno; warna local 5000
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// ===== Services =====

// Database: connection string Postgres jaisा ho (cloud/Neon) to Npgsql,
// warna local SQLite file. Render pe env var "ConnectionStrings__Default" set hoga.
var rawConn = builder.Configuration.GetConnectionString("Default") ?? "Data Source=restaurant.db";
var isPostgres = rawConn.StartsWith("postgres", StringComparison.OrdinalIgnoreCase)
                 || rawConn.Contains("Host=", StringComparison.OrdinalIgnoreCase);

// Neon/Render "postgresql://user:pass@host/db" URL ko Npgsql format me badlo
static string ToNpgsql(string conn)
{
    if (!conn.StartsWith("postgres://") && !conn.StartsWith("postgresql://")) return conn;
    var uri = new Uri(conn);
    var parts = uri.UserInfo.Split(':', 2);
    var db = uri.AbsolutePath.TrimStart('/');
    var port = uri.Port > 0 ? uri.Port : 5432;
    return $"Host={uri.Host};Port={port};Database={db};Username={parts[0]};" +
           $"Password={Uri.UnescapeDataString(parts[1])};SSL Mode=Require;Trust Server Certificate=true";
}

builder.Services.AddDbContext<AppDbContext>(opt =>
{
    if (isPostgres) opt.UseNpgsql(ToNpgsql(rawConn));
    else opt.UseSqlite(rawConn);
});

builder.Services.AddControllers().AddJsonOptions(opt =>
{
    // enum ko number ki jagah string me bhejo: "Pending", "Delivered" etc.
    opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    // Order -> Items -> Order wale circular loop se bacho
    opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Swagger UI ke liye (API test/check karne wala interactive page)
builder.Services.AddSwaggerGen();

// Razorpay API call ke liye HttpClient
builder.Services.AddHttpClient();

// OTP store (ek hi instance poore app me)
builder.Services.AddSingleton<OtpService>();

// CORS — Angular/Ionic dev server se request allow karne ke liye
const string CorsPolicy = "AllowFrontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

// ===== DB create + seed =====
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    DbSeeder.Seed(db);
    DbSeeder.SeedUsers(db);
    DbSeeder.SeedDiscounts(db);
}

// ===== Middleware =====
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Swagger UI -> http://localhost:5000/swagger  (yahan saare API test kar sakte ho)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Restaurant API v1");
    c.RoutePrefix = "swagger";
});

// Angular UI (wwwroot me built app) serve karo — wahi URL pe app khulega
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors(CorsPolicy);
app.MapControllers();

// SPA routing: jo route API/file na ho, Angular ka index.html bhejo
app.MapFallbackToFile("index.html");

app.Run();
