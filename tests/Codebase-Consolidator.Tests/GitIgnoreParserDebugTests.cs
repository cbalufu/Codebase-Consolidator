using Xunit;
using System.IO;
using Microsoft.Extensions.FileSystemGlobbing;

namespace CodebaseConsolidator.Tests;

/// <summary>
/// Tests to understand how Microsoft.Extensions.FileSystemGlobbing works
/// and fix the GitIgnoreParser implementation
/// </summary>
public class GitIgnoreParserDebugTests
{
    [Fact]
    public void TestMicrosoftMatcher_Understanding()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create test files
            var logFile = Path.Combine(tempDir, "debug.log");
            File.WriteAllText(logFile, "log content");

            var txtFile = Path.Combine(tempDir, "readme.txt");
            File.WriteAllText(txtFile, "readme content");

            // Test basic matcher behavior
            var matcher = new Matcher(StringComparison.OrdinalIgnoreCase);

            // Add include patterns (what we want to match)
            matcher.AddInclude("**/*");  // Include everything

            // Add exclude patterns (what we want to exclude)
            matcher.AddExclude("*.log"); // Exclude log files

            // Test individual file matching
            var logMatches = matcher.Match(Path.GetRelativePath(tempDir, logFile));
            var txtMatches = matcher.Match(Path.GetRelativePath(tempDir, txtFile));

            Assert.False(logMatches.HasMatches); // log file should be excluded
            Assert.True(txtMatches.HasMatches); // txt file should be included
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Fact]
    public void TestMicrosoftMatcher_ExcludeOnly()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create test files
            var logFile = Path.Combine(tempDir, "debug.log");
            File.WriteAllText(logFile, "log content");

            var txtFile = Path.Combine(tempDir, "readme.txt");
            File.WriteAllText(txtFile, "readme content");

            // Test matcher with only exclude patterns
            var matcher = new Matcher(StringComparison.OrdinalIgnoreCase);
            matcher.AddExclude("*.log"); // Only exclude log files

            var logMatches = matcher.Match(Path.GetRelativePath(tempDir, logFile));
            var txtMatches = matcher.Match(Path.GetRelativePath(tempDir, txtFile));

            // When only exclude patterns are used, Match returns HasMatches=false for all files
            // because there are no include patterns
            Assert.False(logMatches.HasMatches); // No include patterns, so no matches
            Assert.False(txtMatches.HasMatches); // No include patterns, so no matches

            // This means the GitIgnoreParser needs to work differently!
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }
}
