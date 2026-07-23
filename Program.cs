using Spectre.Console;

using Anvil.Structures;

namespace AnvilExample;

public static class Program
{
    public static void Main(string[] args)
    {
        // 1. Setup Console
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // 2. Parse Arguments
        if (args.Any(argument =>
                argument.Equals("--help", StringComparison.OrdinalIgnoreCase)
                || argument.Equals("-h", StringComparison.OrdinalIgnoreCase)))
        {
            PrintUsage();
            return;
        }

        var filePath = args.FirstOrDefault(a => !a.StartsWith("-")) ?? "Test.class";
        var useClassic = HasOption(args, "--classic", "-c");
        var showAll = HasOption(args, "--all", "-a");
        var showInstructions =
            showAll || HasOption(args, "--instructions", "-i");
        var showControlFlow =
            showAll
            || HasOption(args, "--control-flow", "--cfg")
            || HasOption(args, "-g");
        var roundTrip = showAll || HasOption(args, "--roundtrip", "-r");

        if (!File.Exists(filePath))
        {
            if (useClassic)
            {
                Console.WriteLine($"[Error] File not found: {filePath}");
            }
            else
            {
                AnsiConsole.MarkupLine(
                    $"[red bold]Error:[/] File not found: "
                    + $"[yellow]{Markup.Escape(filePath)}[/]");
            }
            return;
        }

        // 3. Execution
        try
        {
            if (useClassic)
            {
                RunClassic(
                    filePath,
                    showInstructions,
                    showControlFlow,
                    roundTrip);
            }
            else
            {
                RunModern(
                    filePath,
                    showInstructions,
                    showControlFlow,
                    roundTrip);
            }
        }
        catch (Exception ex)
        {
            if (useClassic)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[Fatal Error] {ex.GetType().Name}: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
            }
            else
            {
                AnsiConsole.Write(new Rule("[red]Fatal Error[/]"));
                AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
            }
        }
    }

    private static bool HasOption(
        IEnumerable<string> arguments,
        params string[] options)
    {
        return arguments.Any(argument => options.Any(option =>
            argument.Equals(option, StringComparison.OrdinalIgnoreCase)));
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage: dotnet run -- [class-file] [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -c, --classic       Use plain text output.");
        Console.WriteLine("  -i, --instructions  Show decoded instructions and raw bytecode.");
        Console.WriteLine("  -g, --cfg, --control-flow");
        Console.WriteLine("                      Show basic blocks and control-flow edges.");
        Console.WriteLine("  -r, --roundtrip     Rebuild each method and compare bytecode.");
        Console.WriteLine("  -a, --all           Enable instructions, CFG, and round-trip output.");
        Console.WriteLine("  -h, --help          Show this help.");
    }

    private static void RunClassic(
        string filePath,
        bool showInstructions,
        bool showControlFlow,
        bool roundTrip)
    {
        Console.WriteLine($"Parsing: {Path.GetFullPath(filePath)}");
        Console.WriteLine(new string('-', 50));

        using var fs = File.OpenRead(filePath);
        var classFile = ClassFile.Read(fs);

        var inspector = new ClassicClassInspector(classFile);
        inspector.Display(showInstructions, showControlFlow, roundTrip);

        Console.WriteLine(new string('-', 50));
        Console.WriteLine("Parse Completed Successfully.");
    }

    private static void RunModern(
        string filePath,
        bool showInstructions,
        bool showControlFlow,
        bool roundTrip)
    {
        AnsiConsole.Write(new FigletText("Anvil Debugger").Color(Color.Cyan1));
        
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .Start(
                $"Parsing [green]{Markup.Escape(Path.GetFileName(filePath))}[/]...",
                _ =>
            {
                using var fs = File.OpenRead(filePath);
                var classFile = ClassFile.Read(fs);

                AnsiConsole.MarkupLine(
                    $"[green]Successfully parsed: "
                    + $"{Markup.Escape(Path.GetFullPath(filePath))}[/]");
                AnsiConsole.Write(new Rule());

                var inspector = new ClassInspector(classFile);
                inspector.Display(
                    showInstructions,
                    showControlFlow,
                    roundTrip);
            });
    }
}
