using FileManager.Cli.Commands;

if (args.Length == 0)
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  watch <path>");
    return;
}

switch (args[0].ToLowerInvariant())
{
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

    default:
        Console.WriteLine("Unknown command.");
        break;
}