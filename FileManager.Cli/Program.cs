using FileManager.Cli.Commands;
using FileManager.Application;
using FileManager.Cli.Commands;
using FileManager.Infrastructure;
using FileManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

if (args.Length == 0)
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  index <path>");
    Console.WriteLine("  watch <path>");
    Console.WriteLine("  duplicates");
    Console.WriteLine("  dashboard");
    Console.WriteLine("  search [--name x] [--ext x] [--path x] [--after date] [--before date]");
    return;
}

var services = new ServiceCollection();

services.AddFileManagerInfrastructure();
services.AddFileManagerApplication();
services.AddTransient<IndexCommand>();
services.AddTransient<WatchCommand>();
services.AddTransient<DuplicatesCommand>();
services.AddTransient<SearchCommand>();
services.AddTransient<DashboardCommand>();

await using var provider = services.BuildServiceProvider();

using (var scope = provider.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<FileManagerDbContext>();
    context.Database.Migrate();
}

switch (args[0].ToLowerInvariant())
{
    case "index":
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Missing path");
                return;
            }

            using var scope = provider.CreateScope();

            var indexCommand = scope.ServiceProvider.GetRequiredService<IndexCommand>();

            await indexCommand.ExecuteAsync(args[1]);

            break;
        }

    case "watch":
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Missing path");
                return;
            }

            using var scope = provider.CreateScope();

            var command = scope.ServiceProvider.GetRequiredService<WatchCommand>();

            await command.ExecuteAsync(args[1]);

            break;
        }

    case "duplicates":
        {
            using var scope = provider.CreateScope();

            var duplicatesCommand = scope.ServiceProvider.GetRequiredService<DuplicatesCommand>();

            await duplicatesCommand.ExecuteAsync();

            break;
        }

    case "search":
        {
            using var scope = provider.CreateScope();

            var searchCommand = scope.ServiceProvider.GetRequiredService<SearchCommand>();

            await searchCommand.ExecuteAsync(args.Skip(1).ToArray());

            break;
        }

    case "dashboard":
        {
            using var scope = provider.CreateScope();

            var dashboardCommand = scope.ServiceProvider.GetRequiredService<DashboardCommand>();

            await dashboardCommand.ExecuteAsync();

            break;
        }

    default:
        Console.WriteLine("Unknown command.");
        break;
}