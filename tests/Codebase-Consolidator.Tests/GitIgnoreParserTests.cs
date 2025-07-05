using Xunit;
using System.IO;

namespace CodebaseConsolidator.Tests;

public class GitIgnoreParserTests
{
    [Fact]
    public void GitIgnoreParser_ShouldIgnoreLogFiles()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create .gitignore with log pattern
            var gitignorePath = Path.Combine(tempDir, ".gitignore");
            File.WriteAllText(gitignorePath, "*.log");

            // Create test files
            var logFile = Path.Combine(tempDir, "debug.log");
            File.WriteAllText(logFile, "log content");

            var normalFile = Path.Combine(tempDir, "normal.txt");
            File.WriteAllText(normalFile, "normal content");

            var parser = new GitIgnoreParser(tempDir);

            // Act & Assert
            Assert.True(parser.IsIgnored(logFile), "Log file should be ignored");
            Assert.False(parser.IsIgnored(normalFile), "Normal file should not be ignored");
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Fact]
    public void GitIgnoreParser_ShouldIgnoreDirectories()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create .gitignore with directory pattern
            var gitignorePath = Path.Combine(tempDir, ".gitignore");
            File.WriteAllText(gitignorePath, "bin/");

            // Create test directories and files
            var binDir = Path.Combine(tempDir, "bin");
            Directory.CreateDirectory(binDir);
            var binFile = Path.Combine(binDir, "output.dll");
            File.WriteAllText(binFile, "binary content");

            var srcDir = Path.Combine(tempDir, "src");
            Directory.CreateDirectory(srcDir);
            var srcFile = Path.Combine(srcDir, "program.cs");
            File.WriteAllText(srcFile, "source content");

            var parser = new GitIgnoreParser(tempDir);

            // Act & Assert
            Assert.True(parser.IsIgnored(binFile), "File in bin directory should be ignored");
            Assert.False(parser.IsIgnored(srcFile), "File in src directory should not be ignored");
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Fact]
    public void GitIgnoreParser_ShouldHandleNestedGitIgnores()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create root .gitignore
            var rootGitignorePath = Path.Combine(tempDir, ".gitignore");
            File.WriteAllText(rootGitignorePath, "*.log");

            // Create nested directory with its own .gitignore
            var subDir = Path.Combine(tempDir, "subproject");
            Directory.CreateDirectory(subDir);
            var subGitignorePath = Path.Combine(subDir, ".gitignore");
            File.WriteAllText(subGitignorePath, "*.tmp");

            // Create test files
            var logFile = Path.Combine(tempDir, "debug.log");
            File.WriteAllText(logFile, "log content");

            var tmpFile = Path.Combine(subDir, "temp.tmp");
            File.WriteAllText(tmpFile, "temp content");

            var normalFile = Path.Combine(subDir, "code.cs");
            File.WriteAllText(normalFile, "code content");

            var parser = new GitIgnoreParser(tempDir);

            // Act & Assert
            Assert.True(parser.IsIgnored(logFile), "Log file should be ignored by root .gitignore");
            Assert.True(parser.IsIgnored(tmpFile), "Temp file should be ignored by nested .gitignore");
            Assert.False(parser.IsIgnored(normalFile), "Normal file should not be ignored");
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Fact]
    public void GitIgnoreParser_ShouldHandleIncludePatterns()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create .gitignore that excludes all .txt files
            var gitignorePath = Path.Combine(tempDir, ".gitignore");
            File.WriteAllText(gitignorePath, "*.txt");

            // Create test files
            var txtFile = Path.Combine(tempDir, "readme.txt");
            File.WriteAllText(txtFile, "readme content");

            var parser = new GitIgnoreParser(tempDir);

            // Add include pattern for specific txt file
            parser.AddIncludePatterns(new[] { "readme.txt" });

            // Act & Assert
            Assert.False(parser.IsIgnored(txtFile), "Specifically included file should not be ignored despite matching exclude pattern");
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Fact]
    public void GitIgnoreParser_ShouldIgnoreCommonDefaults()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // No .gitignore file - should still ignore common defaults
            var binDir = Path.Combine(tempDir, "bin");
            Directory.CreateDirectory(binDir);
            var binFile = Path.Combine(binDir, "app.exe");
            File.WriteAllText(binFile, "binary");

            var objDir = Path.Combine(tempDir, "obj");
            Directory.CreateDirectory(objDir);
            var objFile = Path.Combine(objDir, "temp.obj");
            File.WriteAllText(objFile, "object");

            var vsDir = Path.Combine(tempDir, ".vs");
            Directory.CreateDirectory(vsDir);
            var vsFile = Path.Combine(vsDir, "config.json");
            File.WriteAllText(vsFile, "{}");

            var parser = new GitIgnoreParser(tempDir);

            // Act & Assert
            Assert.True(parser.IsIgnored(binFile), "Files in bin directory should be ignored by default");
            Assert.True(parser.IsIgnored(objFile), "Files in obj directory should be ignored by default");
            Assert.True(parser.IsIgnored(vsFile), "Files in .vs directory should be ignored by default");
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }
}
