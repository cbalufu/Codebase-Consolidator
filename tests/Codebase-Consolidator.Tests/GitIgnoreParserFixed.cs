using Microsoft.Extensions.FileSystemGlobbing;
using Serilog;

namespace CodebaseConsolidator;

/// <summary>
/// A helper class to parse .gitignore files and determine if a path should be ignored.
/// Fixed version that properly handles Microsoft.Extensions.FileSystemGlobbing behavior.
/// </summary>
public class GitIgnoreParserFixed
{
    private readonly string _rootDirectory;
    private readonly List<string> _excludePatterns;
    private readonly List<string> _includePatterns;

    public GitIgnoreParserFixed(string rootDirectory)
    {
        _rootDirectory = Path.GetFullPath(rootDirectory);
        _excludePatterns = new List<string>();
        _includePatterns = new List<string>();

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
    /// Adds a collection of glob patterns to exclude.
    /// </summary>
    public void AddPatterns(IEnumerable<string> patterns)
    {
        _excludePatterns.AddRange(patterns);
    }

    /// <summary>
    /// Adds a collection of include patterns that override exclusion rules.
    /// </summary>
    public void AddIncludePatterns(IEnumerable<string> patterns)
    {
        _includePatterns.AddRange(patterns);
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

            // Process patterns similar to original but store them differently
            var processedPatterns = patterns.Select(p => p.StartsWith('/') ? p.Substring(1) : $"**/{p}");
            _excludePatterns.AddRange(processedPatterns);
        }
    }

    /// <summary>
    /// Checks if a given absolute file path is ignored by the loaded patterns.
    /// </summary>
    public bool IsIgnored(string absolutePath)
    {
        var relativePath = Path.GetRelativePath(_rootDirectory, absolutePath);

        // If it matches an explicit include, it's NOT ignored, regardless of other rules.
        if (_includePatterns.Count > 0)
        {
            var includeMatcher = new Matcher(StringComparison.OrdinalIgnoreCase);
            foreach (var pattern in _includePatterns)
            {
                includeMatcher.AddInclude(pattern);
            }

            if (includeMatcher.Match(relativePath).HasMatches)
            {
                return false;
            }
        }

        // Check if file matches any exclude pattern
        if (_excludePatterns.Count > 0)
        {
            var excludeMatcher = new Matcher(StringComparison.OrdinalIgnoreCase);
            foreach (var pattern in _excludePatterns)
            {
                excludeMatcher.AddInclude(pattern); // Use AddInclude to check if pattern matches
            }

            return excludeMatcher.Match(relativePath).HasMatches;
        }

        return false; // If no patterns, don't ignore
    }
}
