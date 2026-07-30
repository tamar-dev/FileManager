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

app.MapGet("/api/duplicates", async (IDuplicateAppService duplicateService) =>
{
    var results = await duplicateService.GetDuplicateReportAsync();
    return Results.Ok(results);
});

app.MapGet("/api/indexed-locations", async (IIndexedLocationAppService locationService) =>
{
    var results = await locationService.GetAllAsync();
    return Results.Ok(results);
});

app.MapPost("/api/indexed-locations", async (
    AddIndexedLocationDto dto,
    IIndexedLocationAppService locationService) =>
{
    var result = await locationService.AddAsync(dto);

    if (!result.Success)
    {
        if (result.IsDuplicate)
        {
            return Results.Conflict(new { error = result.ErrorMessage });
        }

        return Results.BadRequest(new { error = result.ErrorMessage });
    }

    return Results.Ok(result.Location);
});

app.MapDelete("/api/indexed-locations/{id:guid}", async (
    Guid id,
    IIndexedLocationAppService locationService) =>
{
    var removed = await locationService.RemoveAsync(id);

    return removed ? Results.NoContent() : Results.NotFound();
});

app.MapPost("/api/index", (
    IndexRequestDto request,
    IIndexingOrchestrationAppService orchestrationService) =>
{
    if (string.IsNullOrWhiteSpace(request.Path))
    {
        return Results.BadRequest(new { error = "Path is required." });
    }

    if (!Directory.Exists(request.Path))
    {
        return Results.BadRequest(new { error = $"Directory '{request.Path}' does not exist." });
    }

    var started = orchestrationService.TryStartIndexing(request.Path);

    return started
        ? Results.Accepted()
        : Results.Conflict(new { error = "Indexing is already running." });
});

app.MapGet("/api/index/status", (IIndexStatusService statusService) =>
{
    var status = statusService.GetStatus();
    return Results.Ok(status);
});

app.MapGet("/api/virtual-folders", async (IVirtualFolderAppService virtualFolderService) =>
{
    var tree = await virtualFolderService.GetTreeAsync();
    return Results.Ok(tree);
});

app.MapGet("/api/virtual-folders/{id:guid}", async (Guid id, IVirtualFolderAppService virtualFolderService) =>
{
    var folder = await virtualFolderService.GetByIdAsync(id);
    return folder is null ? Results.NotFound() : Results.Ok(folder);
});

app.MapPost("/api/virtual-folders", async (CreateVirtualFolderDto dto, IVirtualFolderAppService virtualFolderService) =>
{
    var result = await virtualFolderService.CreateAsync(dto);

    if (!result.Success)
    {
        if (result.IsConflict)
        {
            return Results.Conflict(new { error = result.ErrorMessage });
        }

        if (result.IsNotFound)
        {
            return Results.NotFound(new { error = result.ErrorMessage });
        }

        return Results.BadRequest(new { error = result.ErrorMessage });
    }

    return Results.Ok(result.Folder);
});

app.MapPatch("/api/virtual-folders/{id:guid}", async (Guid id, UpdateVirtualFolderDto dto, IVirtualFolderAppService virtualFolderService) =>
{
    var result = await virtualFolderService.UpdateAsync(id, dto);

    if (!result.Success)
    {
        if (result.IsConflict)
        {
            return Results.Conflict(new { error = result.ErrorMessage });
        }

        if (result.IsNotFound)
        {
            return Results.NotFound(new { error = result.ErrorMessage });
        }

        return Results.BadRequest(new { error = result.ErrorMessage });
    }

    return Results.Ok(result.Folder);
});

app.MapDelete("/api/virtual-folders/{id:guid}", async (Guid id, IVirtualFolderAppService virtualFolderService) =>
{
    var result = await virtualFolderService.DeleteAsync(id);

    if (!result.Success)
    {
        if (result.IsConflict)
        {
            return Results.Conflict(new { error = result.ErrorMessage });
        }

        if (result.IsNotFound)
        {
            return Results.NotFound(new { error = result.ErrorMessage });
        }

        return Results.BadRequest(new { error = result.ErrorMessage });
    }

    return Results.NoContent();
});

app.MapGet("/api/virtual-folders/{id:guid}/files", async (Guid id, IVirtualFolderAppService virtualFolderService) =>
{
    var files = await virtualFolderService.GetFilesInFolderAsync(id);
    return Results.Ok(files);
});

app.MapPost("/api/virtual-folders/{id:guid}/files", async (Guid id, AddVirtualFolderFileDto dto, IVirtualFolderAppService virtualFolderService) =>
{
    var result = await virtualFolderService.AddFileAsync(id, dto.FileId);

    if (!result.Success)
    {
        if (result.IsNotFound)
        {
            return Results.NotFound(new { error = result.ErrorMessage });
        }

        return Results.BadRequest(new { error = result.ErrorMessage });
    }

    return Results.Ok();
});

app.MapDelete("/api/virtual-folders/{id:guid}/files/{fileId:guid}", async (Guid id, Guid fileId, IVirtualFolderAppService virtualFolderService) =>
{
    var result = await virtualFolderService.RemoveFileAsync(id, fileId);

    if (!result.Success)
    {
        if (result.IsNotFound)
        {
            return Results.NotFound(new { error = result.ErrorMessage });
        }

        return Results.BadRequest(new { error = result.ErrorMessage });
    }

    return Results.NoContent();
});

app.MapGet("/api/files/{fileId:guid}/virtual-folders", async (Guid fileId, IVirtualFolderAppService virtualFolderService) =>
{
    var folders = await virtualFolderService.GetFoldersForFileAsync(fileId);
    return Results.Ok(folders);
});

app.Run();

public record IndexRequestDto(string Path);