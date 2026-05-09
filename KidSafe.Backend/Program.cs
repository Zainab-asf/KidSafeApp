using System.Text;
using KidSafe.Backend.Common;
using KidSafe.Backend.Data;
using KidSafe.Backend.Data.Entities;
using KidSafe.Backend.Hubs;
using KidSafe.Backend.Middleware;
using KidSafe.Backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── JWT Auth ──────────────────────────────────────────────────────────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, ValidateAudience = true,
            ValidateLifetime = true, ValidateIssuerSigningKey = true,
            ValidIssuer    = builder.Configuration["Jwt:Issuer"],
            ValidAudience  = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
        // Allow JWT via query string for SignalR
        opt.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var token = ctx.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(token) &&
                    ctx.HttpContext.Request.Path.StartsWithSegments("/hubs/chat"))
                    ctx.Token = token;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddSignalR();

// ── AI HTTP client ────────────────────────────────────────────────────────────
builder.Services.AddHttpClient<IAIService, AIService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["AI:BaseUrl"]
                                 ?? "http://localhost:8000");
    client.Timeout = TimeSpan.FromSeconds(15);
});

// ── App services ──────────────────────────────────────────────────────────────
builder.Services.AddSingleton<INotificationService, NotificationService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Async moderation pipeline (queue + background worker)
builder.Services.AddSingleton<ModerationQueue>();
builder.Services.AddHostedService<ModerationWorker>();

// ── CORS ──────────────────────────────────────────────────────────────────────
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
                     ?? ["http://localhost:5001", "https://localhost:5001"];

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("DevOpen", p =>
        p.SetIsOriginAllowed(_ => true).AllowAnyMethod().AllowAnyHeader().AllowCredentials());
    opt.AddDefaultPolicy(p =>
        p.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader().AllowCredentials());
});

// ── Build ─────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── DB init + Admin seed ──────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Drop + recreate so schema always matches entities (dev only)
    // Remove EnsureDeleted before production
    db.Database.EnsureDeleted();
    db.Database.EnsureCreated();
    await SeedAdminAsync(db, app.Configuration);
    await SeedDemoDataAsync(db);
}

app.UseErrorHandling();

if (app.Environment.IsDevelopment())
    app.UseCors("DevOpen");
else
    app.UseCors();

// Serve uploaded files
app.UseStaticFiles();
// Ensure uploads directory exists
Directory.CreateDirectory(Path.Combine(app.Environment.WebRootPath
    ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads"));

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.Run();

// ── Demo seeding (dev only) ───────────────────────────────────────────────────
static async Task SeedDemoDataAsync(AppDbContext db)
{
    async Task AddUser(string email, string password, string name, string role, string avatar = "")
    {
        if (await db.Users.AnyAsync(u => u.Email == email)) return;
        var u = new User
        {
            Email        = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            DisplayName  = name,
            Role         = role,
            Status       = "active",
            AvatarEmoji  = avatar
        };
        db.Users.Add(u);
        db.Rewards.Add(new Reward { User = u });
    }

    await AddUser("teacher@demo.com", "Demo@123!", "Demo Teacher", "Teacher");
    await AddUser("parent@demo.com",  "Demo@123!", "Demo Parent",  "Parent");
    await AddUser("student@demo.com", "Demo@123!", "Demo Student", "Child", "😊");
    await db.SaveChangesAsync();
}

// ── Admin seeding ─────────────────────────────────────────────────────────────
static async Task SeedAdminAsync(AppDbContext db, IConfiguration config)
{
    if (await db.Users.AnyAsync(u => u.Role == "Admin"))
        return;  // already seeded

    var email    = config["Admin:Email"]    ?? "admin@kidsafe.app";
    var password = config["Admin:Password"] ?? "Admin@123!";
    var name     = config["Admin:Name"]     ?? "KidSafe Admin";

    var admin = new User
    {
        Email        = email,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
        DisplayName  = name,
        Role         = "Admin",
        Status       = "active"
    };

    db.Users.Add(admin);
    await db.SaveChangesAsync();
}
