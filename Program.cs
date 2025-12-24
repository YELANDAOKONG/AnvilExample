using Spectre.Console;
using Anvil.Structures;

namespace AnvilExample;

public static class Program
{
    public static void Main(string[] args)
    {
        // Configure console to handle UTF-8 output correctly
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // Title
        AnsiConsole.Write(
            new FigletText("Anvil Debugger")
                .Color(Color.Cyan1));

        var filePath = args.Length > 0 ? args[0] : "Test.class";

        if (!File.Exists(filePath))
        {
            AnsiConsole.MarkupLine($"[red]Error: File not found: {filePath}[/]");
            return;
        }

        try
        {
            // Use Status spinner for loading effect
            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .Start($"Parsing [green]{filePath}[/]...", ctx =>
                {
                    using var fs = File.OpenRead(filePath);
                    var classFile = ClassFile.Read(fs);

                    // Clear spinner and render the inspector
                    AnsiConsole.MarkupLine($"[green]Successfully parsed: {Path.GetFullPath(filePath)}[/]");
                    AnsiConsole.Write(new Rule());
                    
                    var inspector = new ClassInspector(classFile);
                    inspector.Display();
                });
        }
        catch (Exception ex)
        {
            AnsiConsole.Write(new Rule("[red]Fatal Error[/]"));
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
        }
    }
}