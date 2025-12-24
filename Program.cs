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
        var filePath = args.FirstOrDefault(a => !a.StartsWith("-")) ?? "Test.class";
        var useClassic = args.Any(a => a.Equals("--classic", StringComparison.OrdinalIgnoreCase) || a.Equals("-c"));

        if (!File.Exists(filePath))
        {
            if (useClassic)
            {
                Console.WriteLine($"[Error] File not found: {filePath}");
            }
            else
            {
                AnsiConsole.MarkupLine($"[red bold]Error:[/] File not found: [yellow]{filePath}[/]");
            }
            return;
        }

        // 3. Execution
        try
        {
            if (useClassic)
            {
                RunClassic(filePath);
            }
            else
            {
                RunModern(filePath);
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

    private static void RunClassic(string filePath)
    {
        Console.WriteLine($"Parsing: {Path.GetFullPath(filePath)}");
        Console.WriteLine(new string('-', 50));

        using var fs = File.OpenRead(filePath);
        var classFile = ClassFile.Read(fs);

        var inspector = new ClassicClassInspector(classFile);
        inspector.Display();

        Console.WriteLine(new string('-', 50));
        Console.WriteLine("Parse Completed Successfully.");
    }

    private static void RunModern(string filePath)
    {
        AnsiConsole.Write(new FigletText("Anvil Debugger").Color(Color.Cyan1));
        
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .Start($"Parsing [green]{Path.GetFileName(filePath)}[/]...", ctx =>
            {
                using var fs = File.OpenRead(filePath);
                var classFile = ClassFile.Read(fs);

                AnsiConsole.MarkupLine($"[green]Successfully parsed: {Path.GetFullPath(filePath)}[/]");
                AnsiConsole.Write(new Rule());

                var inspector = new ClassInspector(classFile);
                inspector.Display();
            });
    }
}
