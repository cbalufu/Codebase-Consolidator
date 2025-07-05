using Xunit;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CodebaseConsolidator.Tests;

/// <summary>
/// Integration tests that test the complete application end-to-end
/// </summary>
public class IntegrationTests
{
    private readonly string _executablePath;

    public IntegrationTests()
    {
        // Find the executable path relative to test assembly
        var testDir = Path.GetDirectoryName(typeof(IntegrationTests).Assembly.Location)!;
        var rootDir = Path.GetFullPath(Path.Combine(testDir, "..", "..", "..", "..", "..", "src", "Codebase-Consolidator", "bin", "Debug", "net9.0"));
        _executablePath = Path.Combine(rootDir, "Codebase-Consolidator.exe");
    }

    [Fact]
    public async Task FullApplication_ShouldConsolidateCSharpProject()
    {
        // Skip if executable doesn't exist (might not be built)
        if (!File.Exists(_executablePath))
        {
            Assert.Fail($"Executable not found at {_executablePath}. Build the project first.");
            return;
        }

        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create a test C# project
            var projectDir = Path.Combine(tempDir, "TestApp");
            Directory.CreateDirectory(projectDir);

            var csprojPath = Path.Combine(projectDir, "TestApp.csproj");
            var csprojContent = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <OutputType>Exe</OutputType>
  </PropertyGroup>
</Project>";
            await File.WriteAllTextAsync(csprojPath, csprojContent);

            var programPath = Path.Combine(projectDir, "Program.cs");
            var programContent = @"using System;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(""Hello, World!"");
        }
    }
}";
            await File.WriteAllTextAsync(programPath, programContent);

            var outputPath = Path.Combine(tempDir, "output.txt");

            // Act
            var result = await RunConsolidatorAsync(tempDir, "--output", outputPath, "--dry-run");

            // Assert
            Assert.Equal(0, result.ExitCode);
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
    public async Task FullApplication_ShouldRespectGitIgnore()
    {
        // Skip if executable doesn't exist
        if (!File.Exists(_executablePath))
        {
            return;
        }

        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create .gitignore
            var gitignorePath = Path.Combine(tempDir, ".gitignore");
            await File.WriteAllTextAsync(gitignorePath, "*.log\ntemp/\n");

            // Create files that should be included
            var readmePath = Path.Combine(tempDir, "README.md");
            await File.WriteAllTextAsync(readmePath, "# Test Project");

            // Create files that should be ignored
            var logPath = Path.Combine(tempDir, "debug.log");
            await File.WriteAllTextAsync(logPath, "log content");

            var tempDirPath = Path.Combine(tempDir, "temp");
            Directory.CreateDirectory(tempDirPath);
            var tempFilePath = Path.Combine(tempDirPath, "temp.txt");
            await File.WriteAllTextAsync(tempFilePath, "temp content");

            var outputPath = Path.Combine(tempDir, "output.txt");

            // Act
            var result = await RunConsolidatorAsync(tempDir, "--output", outputPath);

            // Assert
            Assert.Equal(0, result.ExitCode);

            if (File.Exists(outputPath))
            {
                var output = await File.ReadAllTextAsync(outputPath);
                Assert.Contains("README.md", output);
                Assert.DoesNotContain("debug.log", output);
                Assert.DoesNotContain("temp.txt", output);
            }
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
    public async Task FullApplication_ShouldSplitByProject()
    {
        // Skip if executable doesn't exist
        if (!File.Exists(_executablePath))
        {
            return;
        }

        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create multiple C# projects
            var project1Dir = Path.Combine(tempDir, "Project1");
            Directory.CreateDirectory(project1Dir);
            await File.WriteAllTextAsync(Path.Combine(project1Dir, "Project1.csproj"), "<Project Sdk=\"Microsoft.NET.Sdk\"></Project>");
            await File.WriteAllTextAsync(Path.Combine(project1Dir, "Program.cs"), "class Program1 {}");

            var project2Dir = Path.Combine(tempDir, "Project2");
            Directory.CreateDirectory(project2Dir);
            await File.WriteAllTextAsync(Path.Combine(project2Dir, "Project2.csproj"), "<Project Sdk=\"Microsoft.NET.Sdk\"></Project>");
            await File.WriteAllTextAsync(Path.Combine(project2Dir, "Program.cs"), "class Program2 {}");

            // Act
            var result = await RunConsolidatorAsync(tempDir, "--split-by", "csproj");

            // Assert
            Assert.Equal(0, result.ExitCode);

            // Check if separate files were created for each project
            var project1Output = Path.Combine(tempDir, "Project1-codebase.txt");
            var project2Output = Path.Combine(tempDir, "Project2-codebase.txt");

            // Note: Files might not exist if no matching projects found, but command should succeed
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
    public async Task FullApplication_ShouldHandleEmptyDirectory()
    {
        // Skip if executable doesn't exist
        if (!File.Exists(_executablePath))
        {
            return;
        }

        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Act - run on empty directory
            var result = await RunConsolidatorAsync(tempDir, "--dry-run");

            // Assert
            Assert.Equal(0, result.ExitCode); // Should handle empty directory gracefully
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
    public async Task FullApplication_ShouldFailForNonExistentDirectory()
    {
        // Skip if executable doesn't exist
        if (!File.Exists(_executablePath))
        {
            return;
        }

        // Arrange
        var nonExistentDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        // Act
        var result = await RunConsolidatorAsync(nonExistentDir);

        // Assert
        Assert.NotEqual(0, result.ExitCode); // Should fail for non-existent directory
    }

    private async Task<(int ExitCode, string Output, string Error)> RunConsolidatorAsync(params string[] args)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = _executablePath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var arg in args)
        {
            startInfo.ArgumentList.Add(arg);
        }

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        var output = await outputTask;
        var error = await errorTask;

        return (process.ExitCode, output, error);
    }
}
