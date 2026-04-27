using Hangfire;
using Hangfire.Community.Dashboard.JobsInsights.Services;
using Hangfire.Community.Dashboard.JobsInsights;
using Hangfire.MemoryStorage;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuración de Hangfire
var hangfireSection = builder.Configuration.GetSection("Hangfire");
var storageType = hangfireSection.GetValue<string>("Storage");
var sqlConn = hangfireSection.GetValue<string>("SqlServerConnection");

builder.Services.AddHangfire(config =>
{
    if (storageType == "SqlServer")
    {
        config.UseSqlServerStorage(sqlConn).UseJobsInsights();
    }
    else
    {
        config.UseMemoryStorage().UseJobsInsights();
    }
});
builder.Services.AddHangfireServer();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi(); // Si usas Minimal APIs
}

app.UseHangfireDashboard(); // Dashboard en /hangfire

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


app.Run();
