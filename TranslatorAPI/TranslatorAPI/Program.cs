using Serilog.Events;
using Serilog;
using TranslatorAPI.Models;
using TranslatorAPI.Services;

var builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
// Add services to the container.
builder.Host.UseSerilog();
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<TranslatorConfig>(builder.Configuration.GetSection("TranslatorService"));
// Read AllowedOrigins from configuration
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();

builder.Services.AddHttpClient<TranslatorService>();
builder.Services.AddScoped<EmployeeService>();
// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
    options.AddPolicy("SignalRCorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins) // Explicit SignalR origins
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // SignalR requires credentials
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors("AllowSpecificOrigin");

app.UseSwagger();

app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseSerilogRequestLogging();  // Use Serilog for logging
app.MapControllers();
app.MapHub<ChatHub>("/chatHub").RequireCors("SignalRCorsPolicy");

app.Run();
// Ensure to flush and close log files properly at the end
Log.CloseAndFlush();
