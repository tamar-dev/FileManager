using FileManager.Cli.Commands;

if (args.Length == 0)
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  index <path>");
    Console.WriteLine("  watch <path>");
    Console.WriteLine("  duplicates");
    return;
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

            var indexCommand = new IndexCommand();

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

            var command = new WatchCommand();

            await command.ExecuteAsync(args[1]);

            break;
        }

    case "duplicates":
        {
            var duplicatesCommand = new DuplicatesCommand();

            await duplicatesCommand.ExecuteAsync();

            break;
        }

    default:
        Console.WriteLine("Unknown command.");
        break;
}