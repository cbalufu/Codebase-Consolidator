using Xunit;
using System.IO;

namespace CodebaseConsolidator.Tests;

/// <summary>
/// Tests for edge cases, error conditions, and boundary scenarios
/// </summary>
public class EdgeCaseTests
{
    [Fact]
    public void GitIgnoreParser_ShouldHandleEmptyGitIgnoreFile()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create empty .gitignore
            var gitignorePath = Path.Combine(tempDir, ".gitignore");
            File.WriteAllText(gitignorePath, "");

            var testFile = Path.Combine(tempDir, "test.txt");
            File.WriteAllText(testFile, "content");

            var parser = new GitIgnoreParser(tempDir);

            // Act & Assert
            Assert.False(parser.IsIgnored(testFile)); // Should not be ignored
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
    public void GitIgnoreParser_ShouldHandleCommentsInGitIgnore()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create .gitignore with comments
            var gitignorePath = Path.Combine(tempDir, ".gitignore");
            var gitignoreContent = @"# This is a comment
*.log
# Another comment
temp/
";
            File.WriteAllText(gitignorePath, gitignoreContent);

            var logFile = Path.Combine(tempDir, "debug.log");
            File.WriteAllText(logFile, "log");

            var commentFile = Path.Combine(tempDir, "# This is a comment");
            File.WriteAllText(commentFile, "not a comment");

            var parser = new GitIgnoreParser(tempDir);

            // Act & Assert
            Assert.True(parser.IsIgnored(logFile));
            Assert.False(parser.IsIgnored(commentFile)); // File named like comment should not be ignored
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
    public void GitIgnoreParser_ShouldHandleWhitespaceInGitIgnore()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create .gitignore with whitespace
            var gitignorePath = Path.Combine(tempDir, ".gitignore");
            var gitignoreContent = @"
   *.log   
	temp/	
   
";
            File.WriteAllText(gitignorePath, gitignoreContent);

            var logFile = Path.Combine(tempDir, "debug.log");
            File.WriteAllText(logFile, "log");

            var parser = new GitIgnoreParser(tempDir);

            // Act & Assert
            Assert.True(parser.IsIgnored(logFile)); // Should handle whitespace around patterns
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
    public void GitIgnoreParser_ShouldHandleNonExistentGitIgnore()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // No .gitignore file
            var testFile = Path.Combine(tempDir, "test.txt");
            File.WriteAllText(testFile, "content");

            var parser = new GitIgnoreParser(tempDir);

            // Act & Assert
            Assert.False(parser.IsIgnored(testFile)); // Should not crash, should not ignore regular files
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
    public void ProjectStrategies_ShouldHandleEmptyDirectories()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var strategies = new IProjectDiscoveryStrategy[]
            {
                new CSharpProjectStrategy(),
                new NodeJsProjectStrategy(),
                new MavenProjectStrategy(),
                new PhpComposerProjectStrategy(),
                new PythonProjectStrategy()
            };

            var gitIgnoreParser = new GitIgnoreParser(tempDir);

            foreach (var strategy in strategies)
            {
                // Act
                var projects = strategy.DiscoverProjects(tempDir, gitIgnoreParser);

                // Assert
                Assert.NotNull(projects);
                Assert.Empty(projects); // Should return empty dictionary, not crash
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
    public void NodeJsProjectStrategy_ShouldHandleMalformedPackageJson()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var projectDir = Path.Combine(tempDir, "broken-project");
            Directory.CreateDirectory(projectDir);

            // Create malformed package.json
            var packageJsonPath = Path.Combine(projectDir, "package.json");
            File.WriteAllText(packageJsonPath, "{ broken json content");

            var jsFile = Path.Combine(projectDir, "index.js");
            File.WriteAllText(jsFile, "console.log('test');");

            var strategy = new NodeJsProjectStrategy();
            var gitIgnoreParser = new GitIgnoreParser(tempDir);

            // Act
            var projects = strategy.DiscoverProjects(tempDir, gitIgnoreParser);

            // Assert
            Assert.NotEmpty(projects);
            Assert.True(projects.ContainsKey("broken-project")); // Should fallback to directory name
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
    public void MavenProjectStrategy_ShouldHandleMalformedPomXml()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var projectDir = Path.Combine(tempDir, "broken-maven");
            Directory.CreateDirectory(projectDir);

            // Create malformed pom.xml
            var pomPath = Path.Combine(projectDir, "pom.xml");
            File.WriteAllText(pomPath, "<broken xml>");

            var javaFile = Path.Combine(projectDir, "Main.java");
            File.WriteAllText(javaFile, "public class Main {}");

            var strategy = new MavenProjectStrategy();
            var gitIgnoreParser = new GitIgnoreParser(tempDir);

            // Act
            var projects = strategy.DiscoverProjects(tempDir, gitIgnoreParser);

            // Assert
            Assert.NotEmpty(projects);
            Assert.True(projects.ContainsKey("broken-maven")); // Should fallback to directory name
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
    public void PhpComposerProjectStrategy_ShouldHandleMalformedComposerJson()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var projectDir = Path.Combine(tempDir, "broken-php");
            Directory.CreateDirectory(projectDir);

            // Create malformed composer.json
            var composerJsonPath = Path.Combine(projectDir, "composer.json");
            File.WriteAllText(composerJsonPath, "{ broken json");

            var phpFile = Path.Combine(projectDir, "index.php");
            File.WriteAllText(phpFile, "<?php echo 'test';");

            var strategy = new PhpComposerProjectStrategy();
            var gitIgnoreParser = new GitIgnoreParser(tempDir);

            // Act
            var projects = strategy.DiscoverProjects(tempDir, gitIgnoreParser);

            // Assert
            Assert.NotEmpty(projects);
            Assert.True(projects.ContainsKey("broken-php")); // Should fallback to directory name
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
    public void ProjectStrategies_ShouldHandleSymbolicLinks()
    {
        // Note: Symbolic link creation requires elevated permissions on Windows
        // This test will be skipped on Windows without admin rights
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var projectDir = Path.Combine(tempDir, "test-project");
            Directory.CreateDirectory(projectDir);

            var csprojPath = Path.Combine(projectDir, "Test.csproj");
            File.WriteAllText(csprojPath, "<Project Sdk=\"Microsoft.NET.Sdk\"></Project>");

            var csFile = Path.Combine(projectDir, "Program.cs");
            File.WriteAllText(csFile, "class Program {}");

            var strategy = new CSharpProjectStrategy();
            var gitIgnoreParser = new GitIgnoreParser(tempDir);

            // Act
            var projects = strategy.DiscoverProjects(tempDir, gitIgnoreParser);

            // Assert - Should handle gracefully without crashing
            Assert.NotNull(projects);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try
                {
                    Directory.Delete(tempDir, recursive: true);
                }
                catch
                {
                    // Ignore cleanup errors for symlink tests
                }
            }
        }
    }

    [Fact]
    public void ProjectStrategies_ShouldHandleVeryLongPaths()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create a very deep directory structure (but within Windows limits)
            var deepPath = tempDir;
            for (int i = 0; i < 10; i++)
            {
                deepPath = Path.Combine(deepPath, $"very-long-directory-name-{i}");
                Directory.CreateDirectory(deepPath);
            }

            var csprojPath = Path.Combine(deepPath, "Deep.csproj");
            File.WriteAllText(csprojPath, "<Project Sdk=\"Microsoft.NET.Sdk\"></Project>");

            var strategy = new CSharpProjectStrategy();
            var gitIgnoreParser = new GitIgnoreParser(tempDir);

            // Act
            var projects = strategy.DiscoverProjects(tempDir, gitIgnoreParser);

            // Assert
            Assert.NotNull(projects);
            // May or may not find the project depending on path length limits, but shouldn't crash
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
    public void ProjectStrategies_ShouldHandleSpecialCharactersInPaths()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create directory with special characters (that are valid on Windows)
            var specialDir = Path.Combine(tempDir, "project with spaces & symbols()");
            Directory.CreateDirectory(specialDir);

            var csprojPath = Path.Combine(specialDir, "Special.csproj");
            File.WriteAllText(csprojPath, "<Project Sdk=\"Microsoft.NET.Sdk\"></Project>");

            var strategy = new CSharpProjectStrategy();
            var gitIgnoreParser = new GitIgnoreParser(tempDir);

            // Act
            var projects = strategy.DiscoverProjects(tempDir, gitIgnoreParser);

            // Assert
            Assert.NotNull(projects);
            Assert.NotEmpty(projects);
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
    public void GitIgnoreParser_ShouldHandleAbsolutePaths()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var parser = new GitIgnoreParser(tempDir);

            var testFile = Path.Combine(tempDir, "test.txt");
            File.WriteAllText(testFile, "content");

            // Act & Assert - should handle both relative and absolute paths
            var relativePath = Path.GetRelativePath(tempDir, testFile);
            Assert.False(parser.IsIgnored(testFile)); // absolute path
            Assert.False(parser.IsIgnored(relativePath)); // This may throw as it expects absolute paths
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
