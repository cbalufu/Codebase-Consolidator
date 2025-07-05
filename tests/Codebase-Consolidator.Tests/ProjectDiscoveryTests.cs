using Xunit;
using System.IO;
using System.Text.Json;

namespace CodebaseConsolidator.Tests;

public class ProjectDiscoveryTests
{
    [Fact]
    public void CSharpProjectStrategy_ShouldDiscoverComplexProject()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var projectDir = Path.Combine(tempDir, "MyApp");
            Directory.CreateDirectory(projectDir);

            // Create a complex .csproj file
            var csprojPath = Path.Combine(projectDir, "MyApp.csproj");
            var csprojContent = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include=""Newtonsoft.Json"" Version=""13.0.3"" />
  </ItemGroup>
</Project>";
            File.WriteAllText(csprojPath, csprojContent);

            // Create multiple source files
            var srcDir = Path.Combine(projectDir, "src");
            Directory.CreateDirectory(srcDir);

            var programPath = Path.Combine(srcDir, "Program.cs");
            File.WriteAllText(programPath, "using System; class Program { static void Main() {} }");

            var modelPath = Path.Combine(srcDir, "Model.cs");
            File.WriteAllText(modelPath, "public class Model { public string Name { get; set; } }");

            var configPath = Path.Combine(projectDir, "appsettings.json");
            File.WriteAllText(configPath, "{ \"ConnectionStrings\": {} }");

            var strategy = new CSharpProjectStrategy();
            var gitIgnoreParser = new GitIgnoreParser(tempDir);

            // Act
            var projects = strategy.DiscoverProjects(tempDir, gitIgnoreParser);

            // Assert
            Assert.NotEmpty(projects);
            Assert.True(projects.ContainsKey("MyApp"));

            var projectFiles = projects["MyApp"];
            Assert.Contains(projectFiles, f => f.EndsWith("MyApp.csproj"));
            Assert.Contains(projectFiles, f => f.EndsWith("Program.cs"));
            Assert.Contains(projectFiles, f => f.EndsWith("Model.cs"));
            Assert.Contains(projectFiles, f => f.EndsWith("appsettings.json"));
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
    public void NodeJsProjectStrategy_ShouldExtractProjectNameFromPackageJson()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var projectDir = Path.Combine(tempDir, "frontend-app");
            Directory.CreateDirectory(projectDir);

            // Create package.json with specific name
            var packageJsonPath = Path.Combine(projectDir, "package.json");
            var packageJson = new
            {
                name = "my-awesome-app",
                version = "2.1.0",
                description = "An awesome application",
                scripts = new { start = "node index.js", test = "jest" },
                dependencies = new { express = "^4.18.0" }
            };
            File.WriteAllText(packageJsonPath, JsonSerializer.Serialize(packageJson, new JsonSerializerOptions { WriteIndented = true }));

            // Create source files
            var srcDir = Path.Combine(projectDir, "src");
            Directory.CreateDirectory(srcDir);

            var indexPath = Path.Combine(srcDir, "index.js");
            File.WriteAllText(indexPath, "const express = require('express'); const app = express();");

            var routesPath = Path.Combine(srcDir, "routes.ts");
            File.WriteAllText(routesPath, "export const routes = [];");

            var stylePath = Path.Combine(projectDir, "style.css");
            File.WriteAllText(stylePath, "body { margin: 0; }");

            var strategy = new NodeJsProjectStrategy();
            var gitIgnoreParser = new GitIgnoreParser(tempDir);

            // Act
            var projects = strategy.DiscoverProjects(tempDir, gitIgnoreParser);

            // Assert
            Assert.NotEmpty(projects);
            Assert.True(projects.ContainsKey("my-awesome-app"));

            var projectFiles = projects["my-awesome-app"];
            Assert.Contains(projectFiles, f => f.EndsWith("package.json"));
            Assert.Contains(projectFiles, f => f.EndsWith("index.js"));
            Assert.Contains(projectFiles, f => f.EndsWith("routes.ts"));
            Assert.Contains(projectFiles, f => f.EndsWith("style.css"));
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
    public void NodeJsProjectStrategy_ShouldFallbackToDirectoryName()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var projectDir = Path.Combine(tempDir, "frontend-app");
            Directory.CreateDirectory(projectDir);

            // Create invalid package.json
            var packageJsonPath = Path.Combine(projectDir, "package.json");
            File.WriteAllText(packageJsonPath, "{ invalid json");

            var indexPath = Path.Combine(projectDir, "index.js");
            File.WriteAllText(indexPath, "console.log('Hello');");

            var strategy = new NodeJsProjectStrategy();
            var gitIgnoreParser = new GitIgnoreParser(tempDir);

            // Act
            var projects = strategy.DiscoverProjects(tempDir, gitIgnoreParser);

            // Assert
            Assert.NotEmpty(projects);
            Assert.True(projects.ContainsKey("frontend-app")); // Should fallback to directory name
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
    public void MavenProjectStrategy_ShouldDiscoverJavaProject()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var projectDir = Path.Combine(tempDir, "my-java-app");
            Directory.CreateDirectory(projectDir);

            // Create pom.xml
            var pomPath = Path.Combine(projectDir, "pom.xml");
            var pomContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<project xmlns=""http://maven.apache.org/POM/4.0.0""
         xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
         xsi:schemaLocation=""http://maven.apache.org/POM/4.0.0 
         http://maven.apache.org/xsd/maven-4.0.0.xsd"">
    <modelVersion>4.0.0</modelVersion>
    <groupId>com.example</groupId>
    <artifactId>awesome-java-app</artifactId>
    <version>1.0.0</version>
</project>";
            File.WriteAllText(pomPath, pomContent);

            // Create Java source files
            var srcMainJava = Path.Combine(projectDir, "src", "main", "java", "com", "example");
            Directory.CreateDirectory(srcMainJava);

            var mainClassPath = Path.Combine(srcMainJava, "Main.java");
            File.WriteAllText(mainClassPath, "package com.example; public class Main { public static void main(String[] args) {} }");

            var utilPath = Path.Combine(srcMainJava, "Utils.java");
            File.WriteAllText(utilPath, "package com.example; public class Utils {}");

            var strategy = new MavenProjectStrategy();
            var gitIgnoreParser = new GitIgnoreParser(tempDir);

            // Act
            var projects = strategy.DiscoverProjects(tempDir, gitIgnoreParser);

            // Assert
            Assert.NotEmpty(projects);
            Assert.True(projects.ContainsKey("awesome-java-app"));

            var projectFiles = projects["awesome-java-app"];
            Assert.Contains(projectFiles, f => f.EndsWith("pom.xml"));
            Assert.Contains(projectFiles, f => f.EndsWith("Main.java"));
            Assert.Contains(projectFiles, f => f.EndsWith("Utils.java"));
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
    public void PhpComposerProjectStrategy_ShouldDiscoverPhpProject()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var projectDir = Path.Combine(tempDir, "my-php-app");
            Directory.CreateDirectory(projectDir);

            // Create composer.json
            var composerJsonPath = Path.Combine(projectDir, "composer.json");
            var composerJson = new
            {
                name = "vendor/awesome-php-app",
                description = "An awesome PHP application",
                require = new { php = "^8.0" }
            };
            File.WriteAllText(composerJsonPath, JsonSerializer.Serialize(composerJson, new JsonSerializerOptions { WriteIndented = true }));

            // Create PHP files
            var srcDir = Path.Combine(projectDir, "src");
            Directory.CreateDirectory(srcDir);

            var indexPath = Path.Combine(srcDir, "index.php");
            File.WriteAllText(indexPath, "<?php echo 'Hello World';");

            var classPath = Path.Combine(srcDir, "MyClass.php");
            File.WriteAllText(classPath, "<?php class MyClass {}");

            var strategy = new PhpComposerProjectStrategy();
            var gitIgnoreParser = new GitIgnoreParser(tempDir);

            // Act
            var projects = strategy.DiscoverProjects(tempDir, gitIgnoreParser);

            // Assert
            Assert.NotEmpty(projects);
            Assert.True(projects.ContainsKey("vendor/awesome-php-app"));

            var projectFiles = projects["vendor/awesome-php-app"];
            Assert.Contains(projectFiles, f => f.EndsWith("composer.json"));
            Assert.Contains(projectFiles, f => f.EndsWith("index.php"));
            Assert.Contains(projectFiles, f => f.EndsWith("MyClass.php"));
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
    public void PythonProjectStrategy_ShouldDiscoverPythonProject()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var projectDir = Path.Combine(tempDir, "my-python-app");
            Directory.CreateDirectory(projectDir);

            // Create pyproject.toml
            var pyprojectPath = Path.Combine(projectDir, "pyproject.toml");
            var pyprojectContent = @"[build-system]
requires = [""setuptools"", ""wheel""]

[project]
name = ""awesome-python-app""
version = ""1.0.0""
description = ""An awesome Python application""";
            File.WriteAllText(pyprojectPath, pyprojectContent);

            // Create Python files
            var srcDir = Path.Combine(projectDir, "src");
            Directory.CreateDirectory(srcDir);

            var mainPath = Path.Combine(srcDir, "main.py");
            File.WriteAllText(mainPath, "def main(): print('Hello World')");

            var utilsPath = Path.Combine(srcDir, "utils.py");
            File.WriteAllText(utilsPath, "def helper(): pass");

            var requirementsPath = Path.Combine(projectDir, "requirements.txt");
            File.WriteAllText(requirementsPath, "requests==2.28.0");

            var strategy = new PythonProjectStrategy();
            var gitIgnoreParser = new GitIgnoreParser(tempDir);

            // Act
            var projects = strategy.DiscoverProjects(tempDir, gitIgnoreParser);

            // Assert
            Assert.NotEmpty(projects);
            Assert.True(projects.ContainsKey("my-python-app"));

            var projectFiles = projects["my-python-app"];
            Assert.Contains(projectFiles, f => f.EndsWith("pyproject.toml"));
            Assert.Contains(projectFiles, f => f.EndsWith("main.py"));
            Assert.Contains(projectFiles, f => f.EndsWith("utils.py"));
            Assert.Contains(projectFiles, f => f.EndsWith("requirements.txt"));
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
    public void PythonProjectStrategy_ShouldFallbackToSetupPy()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var projectDir = Path.Combine(tempDir, "legacy-python-app");
            Directory.CreateDirectory(projectDir);

            // Create setup.py (no pyproject.toml)
            var setupPyPath = Path.Combine(projectDir, "setup.py");
            File.WriteAllText(setupPyPath, "from setuptools import setup; setup(name='legacy-app')");

            var mainPath = Path.Combine(projectDir, "main.py");
            File.WriteAllText(mainPath, "print('Legacy app')");

            var strategy = new PythonProjectStrategy();
            var gitIgnoreParser = new GitIgnoreParser(tempDir);

            // Act
            var projects = strategy.DiscoverProjects(tempDir, gitIgnoreParser);

            // Assert
            Assert.NotEmpty(projects);
            Assert.True(projects.ContainsKey("legacy-python-app"));

            var projectFiles = projects["legacy-python-app"];
            Assert.Contains(projectFiles, f => f.EndsWith("setup.py"));
            Assert.Contains(projectFiles, f => f.EndsWith("main.py"));
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
    public void AllStrategies_ShouldIgnoreFilesBasedOnGitIgnore()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create .gitignore
            var gitignorePath = Path.Combine(tempDir, ".gitignore");
            File.WriteAllText(gitignorePath, "*.log\n**/temp/**\n");

            var projectDir = Path.Combine(tempDir, "TestProject");
            Directory.CreateDirectory(projectDir);

            // Create project files
            var csprojPath = Path.Combine(projectDir, "TestProject.csproj");
            File.WriteAllText(csprojPath, "<Project Sdk=\"Microsoft.NET.Sdk\"></Project>");

            var sourcePath = Path.Combine(projectDir, "Program.cs");
            File.WriteAllText(sourcePath, "class Program {}");

            // Create files that should be ignored
            var logPath = Path.Combine(projectDir, "debug.log");
            File.WriteAllText(logPath, "log content");

            var tempDir2 = Path.Combine(projectDir, "temp");
            Directory.CreateDirectory(tempDir2);
            var tempFilePath = Path.Combine(tempDir2, "temp.txt");
            File.WriteAllText(tempFilePath, "temp content");

            var strategy = new CSharpProjectStrategy();
            var gitIgnoreParser = new GitIgnoreParser(tempDir);

            // Act
            var projects = strategy.DiscoverProjects(tempDir, gitIgnoreParser);

            // Assert
            Assert.NotEmpty(projects);
            var projectFiles = projects["TestProject"];

            Assert.Contains(projectFiles, f => f.EndsWith("TestProject.csproj"));
            Assert.Contains(projectFiles, f => f.EndsWith("Program.cs"));
            Assert.DoesNotContain(projectFiles, f => f.EndsWith("debug.log"));
            Assert.DoesNotContain(projectFiles, f => f.EndsWith("temp.txt"));
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
