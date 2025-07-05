using Xunit;
using System.Reflection;

namespace CodebaseConsolidator.Tests;

public class BasicTests
{
    [Fact]
    public void GitIgnoreParser_ShouldCreateInstance()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Act
            var parser = new GitIgnoreParser(tempDir);

            // Assert
            Assert.NotNull(parser);
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
    public void CSharpProjectStrategy_ShouldHaveCorrectStrategyName()
    {
        // Arrange
        var strategy = new CSharpProjectStrategy();

        // Act
        var name = strategy.StrategyName;

        // Assert
        Assert.Equal("csproj", name);
    }

    [Fact]
    public void NodeJsProjectStrategy_ShouldHaveCorrectStrategyName()
    {
        // Arrange
        var strategy = new NodeJsProjectStrategy();

        // Act
        var name = strategy.StrategyName;

        // Assert
        Assert.Equal("package.json", name);
    }

    [Fact]
    public void MavenProjectStrategy_ShouldHaveCorrectStrategyName()
    {
        // Arrange
        var strategy = new MavenProjectStrategy();

        // Act
        var name = strategy.StrategyName;

        // Assert
        Assert.Equal("pom.xml", name);
    }

    [Fact]
    public void PhpComposerProjectStrategy_ShouldHaveCorrectStrategyName()
    {
        // Arrange
        var strategy = new PhpComposerProjectStrategy();

        // Act
        var name = strategy.StrategyName;

        // Assert
        Assert.Equal("composer.json", name);
    }

    [Fact]
    public void PythonProjectStrategy_ShouldHaveCorrectStrategyName()
    {
        // Arrange
        var strategy = new PythonProjectStrategy();

        // Act
        var name = strategy.StrategyName;

        // Assert
        Assert.Equal("pyproject.toml", name);
    }

    [Fact]
    public void ConsolidateSettings_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var settings = new ConsolidateSettings();

        // Assert
        Assert.NotNull(settings.AdditionalExclusions);
        Assert.NotNull(settings.AdditionalInclusions);
        Assert.False(settings.IncludeBinary);
        Assert.False(settings.DryRun);
        Assert.False(settings.CopyToClipboard);
        Assert.Equal("gpt-4", settings.TokenModel);
    }

    [Fact]
    public void CSharpProjectStrategy_ShouldDiscoverBasicProject()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var projectDir = Path.Combine(tempDir, "TestProject");
            Directory.CreateDirectory(projectDir);

            // Create a basic .csproj file
            var csprojPath = Path.Combine(projectDir, "TestProject.csproj");
            File.WriteAllText(csprojPath, "<Project Sdk=\"Microsoft.NET.Sdk\"></Project>");

            // Create a basic .cs file
            var csPath = Path.Combine(projectDir, "Program.cs");
            File.WriteAllText(csPath, "class Program { static void Main() {} }");

            var strategy = new CSharpProjectStrategy();
            var gitIgnoreParser = new GitIgnoreParser(tempDir);

            // Act
            var projects = strategy.DiscoverProjects(tempDir, gitIgnoreParser);

            // Assert
            Assert.NotEmpty(projects);
            Assert.True(projects.ContainsKey("TestProject"));
            Assert.Contains(projects["TestProject"], f => f.EndsWith("TestProject.csproj"));
            Assert.Contains(projects["TestProject"], f => f.EndsWith("Program.cs"));
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
    public void NodeJsProjectStrategy_ShouldDiscoverBasicProject()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var projectDir = Path.Combine(tempDir, "frontend");
            Directory.CreateDirectory(projectDir);

            // Create a basic package.json file
            var packageJsonPath = Path.Combine(projectDir, "package.json");
            File.WriteAllText(packageJsonPath, "{ \"name\": \"test-app\", \"version\": \"1.0.0\" }");

            // Create a basic .js file
            var jsPath = Path.Combine(projectDir, "index.js");
            File.WriteAllText(jsPath, "console.log('Hello World');");

            var strategy = new NodeJsProjectStrategy();
            var gitIgnoreParser = new GitIgnoreParser(tempDir);

            // Act
            var projects = strategy.DiscoverProjects(tempDir, gitIgnoreParser);

            // Assert
            Assert.NotEmpty(projects);
            Assert.True(projects.ContainsKey("test-app")); // Should use name from package.json
            Assert.Contains(projects["test-app"], f => f.EndsWith("package.json"));
            Assert.Contains(projects["test-app"], f => f.EndsWith("index.js"));
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
    public void GitIgnoreParser_ShouldIgnoreBasicPatterns()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create .gitignore with basic patterns
            var gitignorePath = Path.Combine(tempDir, ".gitignore");
            File.WriteAllText(gitignorePath, "*.log\nbin/\n");

            // Create test files
            var logFile = Path.Combine(tempDir, "debug.log");
            File.WriteAllText(logFile, "log content");

            var binDir = Path.Combine(tempDir, "bin");
            Directory.CreateDirectory(binDir);
            var binFile = Path.Combine(binDir, "output.dll");
            File.WriteAllText(binFile, "binary content");

            var normalFile = Path.Combine(tempDir, "normal.txt");
            File.WriteAllText(normalFile, "normal content");

            var parser = new GitIgnoreParser(tempDir);

            // Act & Assert
            Assert.True(parser.IsIgnored(logFile));
            Assert.True(parser.IsIgnored(binFile));
            Assert.False(parser.IsIgnored(normalFile));
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
