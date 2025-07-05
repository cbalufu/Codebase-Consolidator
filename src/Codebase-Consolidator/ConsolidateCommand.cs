using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;
using TextCopy;
using TiktokenSharp;

namespace CodebaseConsolidator;

public sealed class ConsolidateSettings : CommandSettings
{
    [CommandArgument(0, "[PROJECT_ROOT]")]
    [Description("The root directory of the project to consolidate. Defaults to the current directory.")]
    public string? ProjectRoot { get; set; }

    [CommandOption("-o|--output")]
    [Description("The path for the output text file. If not provided, a default is generated.")]
    public string? OutputFile { get; set; }

    [CommandOption("-e|--exclude")]
    [Description("Additional glob patterns to exclude files or folders. Can be used multiple times.")]
    public string[] AdditionalExclusions { get; set; } = Array.Empty<string>();

    [CommandOption("-i|--include")]
    [Description("Glob patterns for files to forcefully include, even if ignored. Can be used multiple times.")]
    public string[] AdditionalInclusions { get; set; } = Array.Empty<string>();

    [CommandOption("--include-binary")]
    [Description("Forces inclusion of files detected as binary.")]
    [DefaultValue(false)]
    public bool IncludeBinary { get; set; }

    [CommandOption("--dry-run")]
    [Description("Lists the files that will be consolidated without creating the output file.")]
    [DefaultValue(false)]
    public bool DryRun { get; set; }

    [CommandOption("-c|--clipboard")]
    [Description("Copies the consolidated output to the system clipboard instead of a file.")]
    [DefaultValue(false)]
    public bool CopyToClipboard { get; set; }

    [CommandOption("--token-model")]
    [Description("Model to use for token estimation (e.g., 'gpt-4', 'gpt-3.5-turbo').")]
    [DefaultValue("gpt-4")]
    public string TokenModel { get; set; } = "gpt-4";
}

public sealed class ConsolidateCommand : AsyncCommand<ConsolidateSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ConsolidateSettings settings)
    {
        // 1. Validate and Finalize Inputs
        var projectRoot = string.IsNullOrWhiteSpace(settings.ProjectRoot)
            ? Directory.GetCurrentDirectory()
            : Path.GetFullPath(settings.ProjectRoot);

        if (!Directory.Exists(projectRoot))
        {
            AnsiConsole.MarkupLine($"[red]Error: Project root directory '{projectRoot}' not found.[/]");
            return -1;
        }

        var outputFilePath = string.IsNullOrWhiteSpace(settings.OutputFile)
            ? Path.Combine(Directory.GetCurrentDirectory(), $"{new DirectoryInfo(projectRoot).Name}-codebase.txt")
            : Path.GetFullPath(settings.OutputFile);

        // Validation: can't use both clipboard and output file
        if (settings.CopyToClipboard && !string.IsNullOrWhiteSpace(settings.OutputFile))
        {
            AnsiConsole.MarkupLine("[yellow]Warning: --clipboard flag specified with --output. Output will go to clipboard only.[/]");
        }

        AnsiConsole.MarkupLine($"[bold]Project Root:[/] [cyan]{projectRoot}[/]");
        if (!settings.CopyToClipboard)
        {
            AnsiConsole.MarkupLine($"[bold]Output File:[/] [cyan]{outputFilePath}[/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"[bold]Output Target:[/] [yellow]Clipboard[/]");
        }
        AnsiConsole.WriteLine();

        var stopwatch = Stopwatch.StartNew();

        // 2. Discover and Filter Files
        List<string> filesToProcess = new();
        await AnsiConsole.Status()
            .StartAsync("Discovering and filtering files...", async ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                var gitIgnoreParser = new GitIgnoreParser(projectRoot);

                // Add user-defined exclusion patterns
                gitIgnoreParser.AddPatterns(settings.AdditionalExclusions);

                // Add user-defined inclusion patterns (override exclusions)
                gitIgnoreParser.AddIncludePatterns(settings.AdditionalInclusions);

                var allFiles = Directory.EnumerateFiles(projectRoot, "*", SearchOption.AllDirectories);

                foreach (var file in allFiles)
                {
                    if (gitIgnoreParser.IsIgnored(file) || file == outputFilePath)
                    {
                        Log.Debug("Ignoring file: {File}", file);
                        continue;
                    }

                    if (!settings.IncludeBinary && await IsBinaryFile(file))
                    {
                        Log.Debug("Skipping binary file: {File}", file);
                        continue;
                    }

                    filesToProcess.Add(file);
                }
            });

        if (filesToProcess.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]Warning: No files found to process after applying filters.[/]");
            return 0;
        }

        AnsiConsole.MarkupLine($"[green]✅ Found {filesToProcess.Count} files to consolidate.[/]\n");

        // Handle dry-run mode
        if (settings.DryRun)
        {
            AnsiConsole.MarkupLine("[bold yellow]-- DRY RUN MODE --[/]");
            AnsiConsole.MarkupLine("[yellow]The following files would be included:[/]\n");

            var root = new Tree($"[cyan]{Path.GetFileName(projectRoot)} ({filesToProcess.Count} files)[/]");

            // Group files by directory for better visualization
            var filesByDir = filesToProcess
                .GroupBy(f => Path.GetDirectoryName(Path.GetRelativePath(projectRoot, f)) ?? "")
                .OrderBy(g => g.Key);

            foreach (var dirGroup in filesByDir)
            {
                var dirName = string.IsNullOrEmpty(dirGroup.Key) ? "[dim](root)[/]" : dirGroup.Key.Replace('\\', '/');
                var dirNode = root.AddNode($"[blue]{dirName}[/]");

                foreach (var file in dirGroup.OrderBy(f => Path.GetFileName(f)))
                {
                    var fileName = Path.GetFileName(file);
                    dirNode.AddNode($"[green]{fileName}[/]");
                }
            }

            AnsiConsole.Write(root);
            stopwatch.Stop();
            AnsiConsole.MarkupLine($"\n[dim]Dry run completed in {stopwatch.Elapsed.TotalSeconds:F2} seconds.[/]");
            return 0;
        }

        // 3. Consolidate Content
        string consolidatedContent;
        int tokenCount = 0;

        try
        {
            if (settings.CopyToClipboard)
            {
                // For clipboard, we must buffer everything in memory
                var stringBuilder = new StringBuilder();
                await ProcessFiles(filesToProcess, projectRoot, content =>
                {
                    stringBuilder.Append(content);
                    return Task.CompletedTask;
                });
                consolidatedContent = stringBuilder.ToString();

                await ClipboardService.SetTextAsync(consolidatedContent);
                AnsiConsole.MarkupLine("[bold green]✅ Output copied to clipboard.[/]");
            }
            else
            {
                // For file output, we stream directly to minimize memory usage
                await using var writer = new StreamWriter(outputFilePath, false, Encoding.UTF8);
                var contentBuilder = new StringBuilder(); // For token counting

                await ProcessFiles(filesToProcess, projectRoot, async content =>
                {
                    await writer.WriteAsync(content);
                    contentBuilder.Append(content); // Also collect for token counting
                });

                consolidatedContent = contentBuilder.ToString();
                AnsiConsole.MarkupLine($"[bold]Output saved to:[/] [underline blue]{outputFilePath}[/]");
            }

            stopwatch.Stop();

            // 4. Calculate token count
            try
            {
                var encoding = TiktokenSharp.TikToken.GetEncoding("cl100k_base"); // Use the encoding name directly
                tokenCount = encoding.Encode(consolidatedContent).Count;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not calculate token count for model {Model}.", settings.TokenModel);
            }

            // 5. Display summary
            AnsiConsole.MarkupLine($"\n[bold green]✅ Consolidation complete in {stopwatch.Elapsed.TotalSeconds:F2} seconds.[/]");

            var summaryTable = new Table().Border(TableBorder.None).Expand().NoBorder();
            summaryTable.AddColumn(new TableColumn("").Width(20));
            summaryTable.AddColumn(new TableColumn(""));

            summaryTable.AddRow("[bold]Files processed:[/]", $"[green]{filesToProcess.Count:N0}[/]");
            summaryTable.AddRow("[bold]Total characters:[/]", $"[cyan]{consolidatedContent.Length:N0}[/]");

            if (tokenCount > 0)
            {
                summaryTable.AddRow("[bold]Est. token count:[/]", $"[yellow]{tokenCount:N0}[/] [dim]({settings.TokenModel})[/]");

                // Add context window warnings for common models
                var contextLimit = GetContextLimit(settings.TokenModel);
                if (contextLimit > 0)
                {
                    var percentage = (double)tokenCount / contextLimit * 100;
                    var color = percentage > 90 ? "red" : percentage > 70 ? "yellow" : "green";
                    summaryTable.AddRow("[bold]Context usage:[/]", $"[{color}]{percentage:F1}%[/] [dim]of {contextLimit:N0} tokens[/]");
                }
            }

            AnsiConsole.Write(summaryTable);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Log.Error(ex, "Failed during file consolidation and writing.");
            AnsiConsole.MarkupLine($"\n[bold red]❌ Consolidation failed after {stopwatch.Elapsed.TotalSeconds:F2} seconds.[/]");
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenPaths);
            return -1;
        }

        return 0;
    }

    private static async Task<bool> IsBinaryFile(string filePath, int bytesToRead = 1024)
    {
        try
        {
            var buffer = new byte[bytesToRead];
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var bytesRead = await stream.ReadAsync(buffer, 0, Math.Min(buffer.Length, (int)stream.Length));

            // A simple heuristic: check for null bytes, which are rare in text files.
            for (var i = 0; i < bytesRead; i++)
            {
                if (buffer[i] == 0)
                {
                    return true;
                }
            }
        }
        catch (IOException)
        {
            // If we can't read the file, assume it's not something we want to process.
            return true;
        }

        return false;
    }

    /// <summary>
    /// Processes files and writes content using the provided write action.
    /// This allows for streaming to file or buffering for clipboard.
    /// </summary>
    private async Task ProcessFiles(List<string> files, string projectRoot, Func<string, Task> writeAction)
    {
        await AnsiConsole.Progress()
            .Columns(new ProgressBarColumn(), new PercentageColumn(), new RemainingTimeColumn(), new SpinnerColumn(), new TaskDescriptionColumn())
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask("[green]Consolidating files[/]", new ProgressTaskSettings { MaxValue = files.Count });

                foreach (var file in files)
                {
                    var relativePath = Path.GetRelativePath(projectRoot, file).Replace('\\', '/');
                    task.Description = $"Processing [blue]{relativePath}[/]";

                    var header = $"==================================================\nFile: {relativePath}\n==================================================\n\n";
                    await writeAction(header);

                    try
                    {
                        var content = await File.ReadAllTextAsync(file);
                        await writeAction(content);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Could not read file {File}, inserting error message.", file);
                        await writeAction($"[ERROR: Could not read file. Reason: {ex.Message}]");
                    }

                    await writeAction("\n\n\n");
                    task.Increment(1);
                }
            });
    }

    /// <summary>
    /// Gets the context window limit for common LLM models.
    /// </summary>
    private static int GetContextLimit(string modelName)
    {
        return modelName.ToLowerInvariant() switch
        {
            // OpenAI GPT models
            "gpt-4" or "gpt-4-0613" => 8192,
            "gpt-4-32k" or "gpt-4-32k-0613" => 32768,
            "gpt-4-turbo" or "gpt-4-1106-preview" or "gpt-4-0125-preview" => 128000,
            "gpt-4o" or "gpt-4o-2024-05-13" or "gpt-4o-2024-08-06" => 128000,
            "gpt-4o-mini" or "gpt-4o-mini-2024-07-18" => 128000,
            "gpt-3.5-turbo" or "gpt-3.5-turbo-0613" => 4096,
            "gpt-3.5-turbo-16k" or "gpt-3.5-turbo-16k-0613" => 16384,
            "gpt-3.5-turbo-1106" or "gpt-3.5-turbo-0125" => 16385,

            // Anthropic Claude 3 series
            "claude-3-haiku-20240307" => 200000,
            "claude-3-sonnet-20240229" => 200000,
            "claude-3-opus-20240229" => 200000,
            "claude-3-5-sonnet-20241022" => 200000,

            // Anthropic Claude 4 series (hypothetical future models)
            "claude-4-haiku" or "claude-4-haiku-20250101" => 300000,
            "claude-4-sonnet" or "claude-4-sonnet-20250101" => 300000,
            "claude-4-opus" or "claude-4-opus-20250101" => 500000,
            "claude-4-5-sonnet" or "claude-4-5-sonnet-20250101" => 500000,

            // Google Gemini 2.5 series
            "gemini-2.5-flash" or "gemini-2.5-flash-001" => 1000000,
            "gemini-2.5-flash-exp" or "gemini-2.5-flash-exp-0827" => 1000000,
            "gemini-2.5-pro" or "gemini-2.5-pro-001" => 2000000,
            "gemini-2.5-pro-exp" or "gemini-2.5-pro-exp-0827" => 2000000,

            // Google Gemini 1.5 series (for reference)
            "gemini-1.5-flash" or "gemini-1.5-flash-001" or "gemini-1.5-flash-002" => 1000000,
            "gemini-1.5-pro" or "gemini-1.5-pro-001" or "gemini-1.5-pro-002" => 2000000,
            "gemini-1.5-pro-exp" or "gemini-1.5-pro-exp-0801" or "gemini-1.5-pro-exp-0827" => 2000000,

            // Google Gemini Pro (legacy)
            "gemini-pro" => 32000,
            "gemini-pro-vision" => 16000,

            _ => 0 // Unknown model
        };
    }
}