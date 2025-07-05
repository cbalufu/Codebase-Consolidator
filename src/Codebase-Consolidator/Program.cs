using System.Reflection;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;
using CodebaseConsolidator;

// Setup structured logging to a file
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File($"consolidator-log-{DateTime.Now:yyyyMMdd}.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "1.0.0";
AnsiConsole.Write(new FigletText("Codebase Consolidator").Color(Color.Blue));
AnsiConsole.MarkupLine($"[bold blue]v{version}[/]");
Log.Information("Codebase Consolidator started. Version: {Version}", version);

var app = new CommandApp<ConsolidateCommand>();
app.Configure(config =>
{
    config.SetApplicationName("consolidate");
    config.ValidateExamples();
    config.AddExample(new[] { @"C:\Users\MyUser\Projects\MyWebApp" });
    config.AddExample(new[] { @"C:\Projects\MyRepo", "-o", @"C:\Temp\MyRepo.txt" });
    config.AddExample(new[] { @".", "--exclude", "**/*.log", "--exclude", "**/temp/*" });
    config.AddExample(new[] { @".", "--include", "**/appsettings.json", "--exclude", "**/bin/**" });
    config.AddExample(new[] { @".", "--dry-run" });
    config.AddExample(new[] { @".", "--clipboard", "--token-model", "gpt-4o" });
    config.AddExample(new[] { @".", "--token-model", "gemini-2.5-pro" });
    config.AddExample(new[] { @".", "--token-model", "claude-4-sonnet" });

#if DEBUG
    config.PropagateExceptions();
    config.ValidateExamples();
#endif
});

try
{
    return app.Run(args);
}
catch (Exception ex)
{
    Log.Fatal(ex, "A critical error occurred.");
    AnsiConsole.MarkupLine("\n[bold red]❌ Operation failed.[/]");
    AnsiConsole.WriteException(ex, ExceptionFormats.ShortenPaths);
    AnsiConsole.MarkupLine($"[red]See [white]consolidator-log-{DateTime.Now:yyyyMMdd}.txt[/] for full details.[/]");
    return -1;
}
finally
{
    Log.CloseAndFlush();
}
