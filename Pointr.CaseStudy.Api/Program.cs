using Microsoft.EntityFrameworkCore;
using Pointr.Base.Infrastructure;
using Pointr.CaseStudy.Api.Middleware;
using Pointr.CaseStudy.Application;
using Pointr.CaseStudy.Infrastructure;
using Pointr.CaseStudy.Infrastructure.Persistence.Database;

var builder = WebApplication.CreateBuilder(args);

// Swagger + Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<ApiErrorMiddleware>();

// Application & Infrastructure
builder.Services.AddCaseStudyApplication();
builder.Services.AddBaseInfrastructure();
builder.Services.AddCaseStudyInfrastructure(builder.Configuration.GetConnectionString("Default"));

var app = builder.Build();

// Swagger (dev)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

await app.Services.EnsureCaseStudyDatabaseAsync();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

await app.Services.EnsureRowVersionInfrastructureAsync();

// Global error mapping
app.UseMiddleware<ApiErrorMiddleware>();

app.MapControllers();

app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
