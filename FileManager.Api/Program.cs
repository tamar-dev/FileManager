using FileManager.Application;
using FileManager.Application.Services;
using FileManager.Infrastructure;
using FileManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using FileManager.Application.Dtos;
using FileManager.Application.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddFileManagerApplication();
builder.Services.AddFileManagerInfrastructure();


builder.Services.AddCors(options =>
{
    options.AddPolicy("Client", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();
app.UseCors("Client");


using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<FileManagerDbContext>();
    context.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/api/health", () => new
{
    status = "ok",
    application = "FileManager.Api",
    database = FileManagerPaths.DatabasePath,
    timestamp = DateTime.UtcNow
});

app.MapGet("/api/dashboard", async (IDashboardAppService dashboardService) =>
{
    var result = await dashboardService.GetDashboardAsync();
    return Results.Ok(result);
});

app.MapGet("/api/files/search", async (
    string? name,
    string? extension,
    string? path,
    DateTime? modifiedAfter,
    DateTime? modifiedBefore,
    ISearchAppService searchService) =>
{
    var query = new SearchQueryDto
    {
        Name = name,
        Extension = extension,
        Path = path,
        ModifiedAfter = modifiedAfter,
        ModifiedBefore = modifiedBefore
    };

    var results = await searchService.SearchAsync(query);

    return Results.Ok(results);
});
app.Run();