using KidSafeApp.Backend.Data;
using KidSafeApp.Backend.Data.Entities;
using KidSafeApp.Backend.Hubs;
using KidSafeApp.Backend.Middleware;
using KidSafeApp.Backend.Repositories.Users;
using KidSafeApp.Backend.Services;
using KidSafeApp.Backend.Services.Users;
using KidSafeApp.Backend.Services.Chat;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = TokenService.GetTokenValidationParameters(builder.Configuration);

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = (context) =>
        {
            if (context.Request.Path.StartsWithSegments("/hubs/kidsafeapp"))
            {
                var jwt = context.Request.Query["access_token"];
                if (!string.IsNullOrWhiteSpace(jwt))
                {
                    context.Token = jwt;
                }
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Chat")));

builder.Services.AddTransient<TokenService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddSignalR();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddHealthChecks();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DataContext>();
    await db.Database.MigrateAsync();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DataContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

    try
    {
        if (db.Database.CanConnect())
        {
            logger.LogInformation("Database connectivity check succeeded.");
        }
        else
        {
            logger.LogError("Database connectivity check failed.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database connectivity check threw an exception.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Development-only middleware can go here.
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/efcore-docs-pending-changes.
    app.UseHsts();
}

// Add custom global exception handler (must be early in pipeline)
app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapHub<ChatHub>("/hubs/kidsafeapp");
app.MapHealthChecks("/health");
app.MapFallbackToFile("index.html");

app.Run();
