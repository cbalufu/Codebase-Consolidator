using System.Text.Json;
using System.Xml.Linq;

namespace CodebaseConsolidator;

/// <summary>
/// Strategy interface for discovering projects based on different ecosystem markers.
/// </summary>
public interface IProjectDiscoveryStrategy
{
    /// <summary>
    /// The name of the strategy, e.g., "csproj", "package.json", "pom.xml".
    /// </summary>
    string StrategyName { get; }

    /// <summary>
    /// The glob pattern for the main project marker file.
    /// </summary>
    string ProjectMarkerPattern { get; }

    /// <summary>
    /// Discovers all projects and their associated source files within the root directory.
    /// </summary>
    /// <param name="rootDir">The root directory to scan.</param>
    /// <param name="gitIgnoreParser">The parser for filtering out ignored files.</param>
    /// <returns>A dictionary where the key is the project name and the value is a list of its files.</returns>
    Dictionary<string, List<string>> DiscoverProjects(string rootDir, GitIgnoreParser gitIgnoreParser);
}

/// <summary>
/// Strategy for discovering C# projects based on .csproj files.
/// </summary>
public class CSharpProjectStrategy : IProjectDiscoveryStrategy
{
    public string StrategyName => "csproj";
    public string ProjectMarkerPattern => "*.csproj";

    public Dictionary<string, List<string>> DiscoverProjects(string rootDir, GitIgnoreParser gitIgnoreParser)
    {
        var projects = new Dictionary<string, List<string>>();
        var markerFiles = Directory.EnumerateFiles(rootDir, ProjectMarkerPattern, SearchOption.AllDirectories);

        foreach (var markerFile in markerFiles)
        {
            if (gitIgnoreParser.IsIgnored(markerFile)) continue;

            var projDir = Path.GetDirectoryName(markerFile);
            if (projDir == null) continue;

            var projName = Path.GetFileNameWithoutExtension(markerFile);

            var sourceFiles = Directory.EnumerateFiles(projDir, "*", SearchOption.AllDirectories)
                .Where(f => !gitIgnoreParser.IsIgnored(f))
                .Where(f => IsCSharpSourceFile(f))
                .ToList();

            // Add the marker file itself if it's not already included
            if (!sourceFiles.Contains(markerFile))
            {
                sourceFiles.Insert(0, markerFile);
            }

            projects[projName] = sourceFiles;
        }
        return projects;
    }

    private static bool IsCSharpSourceFile(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext is ".cs" or ".csproj" or ".sln" or ".config" or ".json" or ".xml" or ".resx" or ".settings";
    }
}

/// <summary>
/// Strategy for discovering Node.js projects based on package.json files.
/// </summary>
public class NodeJsProjectStrategy : IProjectDiscoveryStrategy
{
    public string StrategyName => "package.json";
    public string ProjectMarkerPattern => "package.json";

    public Dictionary<string, List<string>> DiscoverProjects(string rootDir, GitIgnoreParser gitIgnoreParser)
    {
        var projects = new Dictionary<string, List<string>>();
        var markerFiles = Directory.EnumerateFiles(rootDir, ProjectMarkerPattern, SearchOption.AllDirectories);

        foreach (var markerFile in markerFiles)
        {
            if (gitIgnoreParser.IsIgnored(markerFile)) continue;

            var projDir = Path.GetDirectoryName(markerFile);
            if (projDir == null) continue;

            // Try to extract project name from package.json
            var projName = ExtractNodeProjectName(markerFile) ?? Path.GetFileName(projDir);

            var sourceFiles = Directory.EnumerateFiles(projDir, "*", SearchOption.AllDirectories)
                .Where(f => !gitIgnoreParser.IsIgnored(f))
                .Where(f => IsNodeSourceFile(f))
                .ToList();

            sourceFiles.Insert(0, markerFile);
            projects[projName] = sourceFiles;
        }
        return projects;
    }

    private static string? ExtractNodeProjectName(string packageJsonPath)
    {
        try
        {
            var json = File.ReadAllText(packageJsonPath);
            var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("name", out var nameElement))
            {
                return nameElement.GetString();
            }
        }
        catch (Exception ex)
        {
            Serilog.Log.Warning(ex, "Could not parse package.json at {Path}", packageJsonPath);
        }
        return null;
    }

    private static bool IsNodeSourceFile(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext is ".js" or ".ts" or ".jsx" or ".tsx" or ".json" or ".css" or ".scss" or ".less"
                   or ".html" or ".htm" or ".vue" or ".svelte" or ".md" or ".yml" or ".yaml";
    }
}

/// <summary>
/// Strategy for discovering Java Maven projects based on pom.xml files.
/// </summary>
public class MavenProjectStrategy : IProjectDiscoveryStrategy
{
    public string StrategyName => "pom.xml";
    public string ProjectMarkerPattern => "pom.xml";

    public Dictionary<string, List<string>> DiscoverProjects(string rootDir, GitIgnoreParser gitIgnoreParser)
    {
        var projects = new Dictionary<string, List<string>>();
        var markerFiles = Directory.EnumerateFiles(rootDir, ProjectMarkerPattern, SearchOption.AllDirectories);

        foreach (var markerFile in markerFiles)
        {
            if (gitIgnoreParser.IsIgnored(markerFile)) continue;

            var projDir = Path.GetDirectoryName(markerFile);
            if (projDir == null) continue;

            var projName = ExtractMavenArtifactId(markerFile) ?? Path.GetFileName(projDir);

            var sourceFiles = Directory.EnumerateFiles(projDir, "*", SearchOption.AllDirectories)
                .Where(f => !gitIgnoreParser.IsIgnored(f))
                .Where(f => IsJavaSourceFile(f))
                .ToList();

            sourceFiles.Insert(0, markerFile);
            projects[projName] = sourceFiles;
        }
        return projects;
    }

    private static string? ExtractMavenArtifactId(string pomPath)
    {
        try
        {
            var doc = XDocument.Load(pomPath);
            var ns = doc.Root?.GetDefaultNamespace();
            if (ns != null)
            {
                var artifactId = doc.Root?.Element(ns + "artifactId")?.Value;
                return artifactId;
            }
        }
        catch (Exception ex)
        {
            Serilog.Log.Warning(ex, "Could not parse pom.xml at {Path}", pomPath);
        }
        return null;
    }

    private static bool IsJavaSourceFile(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext is ".java" or ".kt" or ".scala" or ".xml" or ".properties" or ".yml" or ".yaml" or ".json";
    }
}

/// <summary>
/// Strategy for discovering PHP Composer projects based on composer.json files.
/// </summary>
public class PhpComposerProjectStrategy : IProjectDiscoveryStrategy
{
    public string StrategyName => "composer.json";
    public string ProjectMarkerPattern => "composer.json";

    public Dictionary<string, List<string>> DiscoverProjects(string rootDir, GitIgnoreParser gitIgnoreParser)
    {
        var projects = new Dictionary<string, List<string>>();
        var markerFiles = Directory.EnumerateFiles(rootDir, ProjectMarkerPattern, SearchOption.AllDirectories);

        foreach (var markerFile in markerFiles)
        {
            if (gitIgnoreParser.IsIgnored(markerFile)) continue;

            var projDir = Path.GetDirectoryName(markerFile);
            if (projDir == null) continue;

            var projName = ExtractComposerProjectName(markerFile) ?? Path.GetFileName(projDir);

            var sourceFiles = Directory.EnumerateFiles(projDir, "*", SearchOption.AllDirectories)
                .Where(f => !gitIgnoreParser.IsIgnored(f))
                .Where(f => IsPhpSourceFile(f))
                .ToList();

            sourceFiles.Insert(0, markerFile);
            projects[projName] = sourceFiles;
        }
        return projects;
    }

    private static string? ExtractComposerProjectName(string composerJsonPath)
    {
        try
        {
            var json = File.ReadAllText(composerJsonPath);
            var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("name", out var nameElement))
            {
                return nameElement.GetString();
            }
        }
        catch (Exception ex)
        {
            Serilog.Log.Warning(ex, "Could not parse composer.json at {Path}", composerJsonPath);
        }
        return null;
    }

    private static bool IsPhpSourceFile(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext is ".php" or ".phtml" or ".inc" or ".twig" or ".blade.php" or ".json" or ".yml" or ".yaml" or ".xml";
    }
}

/// <summary>
/// Strategy for discovering Python projects based on pyproject.toml files.
/// </summary>
public class PythonProjectStrategy : IProjectDiscoveryStrategy
{
    public string StrategyName => "pyproject.toml";
    public string ProjectMarkerPattern => "pyproject.toml";

    public Dictionary<string, List<string>> DiscoverProjects(string rootDir, GitIgnoreParser gitIgnoreParser)
    {
        var projects = new Dictionary<string, List<string>>();

        // First try pyproject.toml files
        var markerFiles = Directory.EnumerateFiles(rootDir, ProjectMarkerPattern, SearchOption.AllDirectories);

        // If no pyproject.toml found, fall back to setup.py
        if (!markerFiles.Any())
        {
            markerFiles = Directory.EnumerateFiles(rootDir, "setup.py", SearchOption.AllDirectories);
        }

        foreach (var markerFile in markerFiles)
        {
            if (gitIgnoreParser.IsIgnored(markerFile)) continue;

            var projDir = Path.GetDirectoryName(markerFile);
            if (projDir == null) continue;

            var projName = Path.GetFileName(projDir);

            var sourceFiles = Directory.EnumerateFiles(projDir, "*", SearchOption.AllDirectories)
                .Where(f => !gitIgnoreParser.IsIgnored(f))
                .Where(f => IsPythonSourceFile(f))
                .ToList();

            sourceFiles.Insert(0, markerFile);
            projects[projName] = sourceFiles;
        }
        return projects;
    }

    private static bool IsPythonSourceFile(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext is ".py" or ".pyx" or ".pyi" or ".toml" or ".cfg" or ".txt" or ".yml" or ".yaml" or ".json" or ".md";
    }
}

/// <summary>
/// Strategy for discovering Android/Gradle projects based on build.gradle files.
/// </summary>
public class AndroidGradleProjectStrategy : IProjectDiscoveryStrategy
{
    public string StrategyName => "build.gradle";
    public string ProjectMarkerPattern => "build.gradle";

    public Dictionary<string, List<string>> DiscoverProjects(string rootDir, GitIgnoreParser gitIgnoreParser)
    {
        var projects = new Dictionary<string, List<string>>();
        var markerFiles = Directory.EnumerateFiles(rootDir, ProjectMarkerPattern, SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(rootDir, "build.gradle.kts", SearchOption.AllDirectories));

        foreach (var markerFile in markerFiles)
        {
            if (gitIgnoreParser.IsIgnored(markerFile)) continue;

            var projDir = Path.GetDirectoryName(markerFile);
            if (projDir == null) continue;

            // Extract project name from settings.gradle, gradle.properties, or directory name
            var projName = ExtractAndroidProjectName(projDir) ?? Path.GetFileName(projDir);

            var sourceFiles = Directory.EnumerateFiles(projDir, "*", SearchOption.AllDirectories)
                .Where(f => !gitIgnoreParser.IsIgnored(f))
                .Where(f => IsAndroidSourceFile(f))
                .ToList();

            sourceFiles.Insert(0, markerFile);
            projects[projName] = sourceFiles;
        }
        return projects;
    }

    private static string? ExtractAndroidProjectName(string projectDir)
    {
        try
        {
            // Try to find app name from AndroidManifest.xml
            var manifestPaths = new[]
            {
                Path.Combine(projectDir, "src", "main", "AndroidManifest.xml"),
                Path.Combine(projectDir, "app", "src", "main", "AndroidManifest.xml"),
                Path.Combine(projectDir, "AndroidManifest.xml")
            };

            foreach (var manifestPath in manifestPaths)
            {
                if (File.Exists(manifestPath))
                {
                    var manifestContent = File.ReadAllText(manifestPath);
                    var doc = XDocument.Parse(manifestContent);
                    var appElement = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "application");
                    var labelAttr = appElement?.Attributes().FirstOrDefault(x => x.Name.LocalName == "label");
                    if (labelAttr != null && !string.IsNullOrWhiteSpace(labelAttr.Value) && !labelAttr.Value.StartsWith("@"))
                    {
                        return labelAttr.Value;
                    }
                }
            }

            // Try to extract from settings.gradle
            var settingsGradlePath = Path.Combine(projectDir, "settings.gradle");
            if (File.Exists(settingsGradlePath))
            {
                var settingsContent = File.ReadAllText(settingsGradlePath);
                var lines = settingsContent.Split('\n');
                foreach (var line in lines)
                {
                    if (line.Trim().StartsWith("rootProject.name"))
                    {
                        var parts = line.Split('=');
                        if (parts.Length > 1)
                        {
                            return parts[1].Trim().Trim('"', '\'');
                        }
                    }
                }
            }

            // Try to extract from gradle.properties
            var gradlePropsPath = Path.Combine(projectDir, "gradle.properties");
            if (File.Exists(gradlePropsPath))
            {
                var propsContent = File.ReadAllText(gradlePropsPath);
                var lines = propsContent.Split('\n');
                foreach (var line in lines)
                {
                    if (line.Trim().StartsWith("android.application.name") || line.Trim().StartsWith("app.name"))
                    {
                        var parts = line.Split('=');
                        if (parts.Length > 1)
                        {
                            return parts[1].Trim();
                        }
                    }
                }
            }
        }
        catch
        {
            // If parsing fails, we'll fall back to directory name
        }

        return null;
    }

    private static bool IsAndroidSourceFile(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        var fileName = Path.GetFileName(filePath).ToLowerInvariant();

        // Android source files and configuration
        if (ext is ".java" or ".kt" or ".kts" or ".xml" or ".gradle" or ".properties" or ".pro" or ".json" or ".yml" or ".yaml" or ".md")
            return true;

        // Android specific files
        if (fileName is "androidmanifest.xml" or "proguard-rules.pro" or "build.gradle" or "settings.gradle" or "gradle.properties" or "local.properties")
            return true;

        // Resource and asset files
        var dirName = Path.GetFileName(Path.GetDirectoryName(filePath))?.ToLowerInvariant();
        if (dirName?.StartsWith("res") == true || dirName?.StartsWith("assets") == true)
            return true;

        return false;
    }
}
