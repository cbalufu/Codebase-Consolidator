using Xunit;
using System.IO;
using System.Threading.Tasks;
using Spectre.Console.Cli;
using Microsoft.Extensions.DependencyInjection;

namespace CodebaseConsolidator.Tests;

public class ConsolidateCommandTests
{
    [Fact]
    public void ConsolidateSettings_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        var settings = new ConsolidateSettings();

        // Assert
        Assert.NotNull(settings.AdditionalExclusions);
        Assert.Empty(settings.AdditionalExclusions);
        Assert.NotNull(settings.AdditionalInclusions);
        Assert.Empty(settings.AdditionalInclusions);
        Assert.False(settings.IncludeBinary);
        Assert.False(settings.DryRun);
        Assert.False(settings.CopyToClipboard);
        Assert.Equal("gpt-4", settings.TokenModel);
        Assert.Null(settings.SplitBy);
        Assert.Null(settings.ProjectRoot);
        Assert.Null(settings.OutputFile);
    }

    [Fact]
    public async Task ConsolidateCommand_ShouldFailForNonExistentDirectory()
    {
        // Arrange
        var command = new ConsolidateCommand();
        var context = new CommandContext(new ServiceCollection(), new RemainingArguments(Array.Empty<string>(), Array.Empty<string>()), "consolidate");
        var settings = new ConsolidateSettings
        {
            ProjectRoot = @"C:\NonExistentDirectory\That\Does\Not\Exist"
        };

        // Act
        var result = await command.ExecuteAsync(context, settings);

        // Assert
        Assert.Equal(-1, result);
    }

    [Fact]
    public async Task ConsolidateCommand_ShouldFailForInvalidSplitStrategy()
    {
        // Arrange
        var command = new ConsolidateCommand();
        var context = new CommandContext(new ServiceCollection(), new RemainingArguments(Array.Empty<string>(), Array.Empty<string>()), "consolidate");

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var settings = new ConsolidateSettings
            {
                ProjectRoot = tempDir,
                SplitBy = "invalid-strategy"
            };

            // Act
            var result = await command.ExecuteAsync(context, settings);

            // Assert
            Assert.Equal(-1, result);
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
    public async Task ConsolidateCommand_ShouldFailWhenUsingClipboardWithSplitBy()
    {
        // Arrange
        var command = new ConsolidateCommand();
        var context = new CommandContext(new ServiceCollection(), new RemainingArguments(Array.Empty<string>(), Array.Empty<string>()), "consolidate");

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var settings = new ConsolidateSettings
            {
                ProjectRoot = tempDir,
                SplitBy = "csproj",
                CopyToClipboard = true
            };

            // Act
            var result = await command.ExecuteAsync(context, settings);

            // Assert
            Assert.Equal(-1, result);
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
    public async Task ConsolidateCommand_ShouldUseCurrentDirectoryWhenProjectRootIsEmpty()
    {
        // Arrange
        var command = new ConsolidateCommand();
        var context = new CommandContext(new ServiceCollection(), new RemainingArguments(Array.Empty<string>(), Array.Empty<string>()), "consolidate");

        var originalDir = Directory.GetCurrentDirectory();
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Change to temp directory
            Directory.SetCurrentDirectory(tempDir);

            // Create a simple file so directory isn't empty
            File.WriteAllText(Path.Combine(tempDir, "test.txt"), "test content");

            var settings = new ConsolidateSettings
            {
                ProjectRoot = null, // Should use current directory
                DryRun = true // Don't create actual output file
            };

            // Act
            var result = await command.ExecuteAsync(context, settings);

            // Assert
            Assert.Equal(0, result); // Should succeed
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Theory]
    [InlineData("csproj")]
    [InlineData("package.json")]
    [InlineData("pom.xml")]
    [InlineData("composer.json")]
    [InlineData("pyproject.toml")]
    public async Task ConsolidateCommand_ShouldAcceptValidSplitStrategies(string splitStrategy)
    {
        // Arrange
        var command = new ConsolidateCommand();
        var context = new CommandContext(new ServiceCollection(), new RemainingArguments(Array.Empty<string>(), Array.Empty<string>()), "consolidate");

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create appropriate project file for the strategy
            var projectFile = splitStrategy switch
            {
                "csproj" => "TestProject.csproj",
                "package.json" => "package.json",
                "pom.xml" => "pom.xml",
                "composer.json" => "composer.json",
                "pyproject.toml" => "pyproject.toml",
                _ => throw new ArgumentException($"Unknown strategy: {splitStrategy}")
            };

            var projectContent = splitStrategy switch
            {
                "csproj" => "<Project Sdk=\"Microsoft.NET.Sdk\"></Project>",
                "package.json" => "{ \"name\": \"test\", \"version\": \"1.0.0\" }",
                "pom.xml" => "<?xml version=\"1.0\"?><project><modelVersion>4.0.0</modelVersion><groupId>test</groupId><artifactId>test</artifactId><version>1.0</version></project>",
                "composer.json" => "{ \"name\": \"test/test\" }",
                "pyproject.toml" => "[project]\nname = \"test\"",
                _ => throw new ArgumentException($"Unknown strategy: {splitStrategy}")
            };

            File.WriteAllText(Path.Combine(tempDir, projectFile), projectContent);

            var settings = new ConsolidateSettings
            {
                ProjectRoot = tempDir,
                SplitBy = splitStrategy,
                DryRun = true
            };

            // Act
            var result = await command.ExecuteAsync(context, settings);

            // Assert
            Assert.Equal(0, result); // Should succeed
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

public static class TestHelpers
{
    public static ServiceCollection CreateServiceCollection()
    {
        return new ServiceCollection();
    }
}
