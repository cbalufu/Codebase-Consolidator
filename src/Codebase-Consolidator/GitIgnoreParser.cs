using Microsoft.Extensions.FileSystemGlobbing;
using Serilog;

namespace CodebaseConsolidator;

/// <summary>
/// A helper class to parse .gitignore files and determine if a path should be ignored.
/// </summary>
public class GitIgnoreParser
{
    private readonly string _rootDirectory;
    private readonly Matcher _matcher;
    private readonly Matcher _includeMatcher;

    public GitIgnoreParser(string rootDirectory)
    {
        _rootDirectory = Path.GetFullPath(rootDirectory);
        _matcher = new Matcher(StringComparison.OrdinalIgnoreCase);
        _includeMatcher = new Matcher(StringComparison.OrdinalIgnoreCase);

        // Add some common defaults that should always be ignored
        AddPatterns(new[]
        {
            "**/.git/**",
            "**/.vs/**",
            "**/.vscode/**",
            "**/bin/**",
            "**/obj/**"
        });

        LoadAllGitIgnoreFiles();
    }

    /// <summary>
    /// Adds a collection of glob patterns to the matcher.
    /// </summary>
    public void AddPatterns(IEnumerable<string> patterns)
    {
        foreach (var pattern in patterns)
        {
            _matcher.AddInclude(pattern);
        }
    }

    /// <summary>
    /// Adds a collection of include patterns that override exclusion rules.
    /// </summary>
    public void AddIncludePatterns(IEnumerable<string> patterns)
    {
        foreach (var pattern in patterns)
        {
            _includeMatcher.AddInclude(pattern);
        }
    }

    private void LoadAllGitIgnoreFiles()
    {
        var gitIgnoreFiles = Directory.EnumerateFiles(_rootDirectory, ".gitignore", SearchOption.AllDirectories);
        foreach (var file in gitIgnoreFiles)
        {
            Log.Debug("Loading .gitignore file: {File}", file);
            var patterns = File.ReadAllLines(file)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrEmpty(line) && !line.StartsWith('#'));

            // FileSystemGlobbing's Matcher doesn't have a concept of a base directory per pattern.
            // We can simulate it for top-level root patterns like `/logs` by prepending `**/`.
            // This is a simplification but covers the most common cases effectively.
            var processedPatterns = patterns.Select(p => p.StartsWith('/') ? p.Substring(1) : $"**/{p}");

            _matcher.AddExcludePatterns(processedPatterns);
        }
    }

    /// <summary>
    /// Checks if a given absolute file path is ignored by the loaded patterns.
    /// </summary>
    public bool IsIgnored(string absolutePath)
    {
        var relativePath = Path.GetRelativePath(_rootDirectory, absolutePath);

        // If it matches an explicit include, it's NOT ignored, regardless of other rules.
        if (_includeMatcher.Match(relativePath).HasMatches)
        {
            return false;
        }

        // Otherwise, check the exclusion rules.
        return _matcher.Match(relativePath).HasMatches;
    }
}