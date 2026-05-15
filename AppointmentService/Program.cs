using AppointmentService.Data;
using AppointmentService.Repositories;
using AppointmentService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// ── Database (PostgreSQL) ────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── HTTP Client for Schedule-Service communication ───────────────────────────
builder.Services.AddHttpClient("ScheduleService", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ServiceUrls:ScheduleService"]
        ?? "http://localhost:5043");
});


// ── HTTP Client for Provider-Service communication ───────────────────────────
builder.Services.AddHttpClient("ProviderService", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ServiceUrls:ProviderService"]
        ?? "http://localhost:5096");
});


// ── HTTP Client for Auth-Service communication ────────────────────────────────
builder.Services.AddHttpClient("AuthService", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ServiceUrls:AuthService"]
        ?? "http://localhost:5219");
});


builder.Services.AddHttpClient("PaymentService", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ServiceUrls:PaymentService"]
        ?? "http://localhost:5048");
});

// ── Dependency Injection ─────────────────────────────────────────────────────
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IAppointmentService, AppointmentService.Services.AppointmentService>();

// ── CORS ────────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

});


// ── Controllers ──────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── Swagger ──────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MediBook Appointment Service",
        Version = "v1",
        Description = "Manages full appointment booking lifecycle — creation, rescheduling, cancellation, and completion."
    });
});

var app = builder.Build();

// ── Middleware Pipeline ───────────────────────────────────────────────────────
app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "MediBook Appointment Service v1");
    options.RoutePrefix = "swagger";
});

app.MapGet("/", () => "MediBook Appointment Service Running ✅");
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "AppointmentService" }));

app.UseAuthorization();
app.MapControllers();

// ── Auto Migrate on Startup ───────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();