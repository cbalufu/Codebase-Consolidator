# Codebase Consolidator

[![Build](https://img.shields.io/badge/build-passing-brightgreen.svg)]()
[![Tests](https://img.shields.io/badge/tests-54%20passing-brightgreen.svg)]()
[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)]()
[![License](https://img.shields.io/badge/license-MIT-blue.svg)]()

A powerful command-line tool that consolidates your entire codebase into a single text file or splits it by project types, making it easy to share with AI assistants, conduct code reviews, or create project documentation.

## 🌟 Features

- **Smart Project Discovery**: Automatically detects and organizes projects by type (C#, Node.js, Maven, PHP, Python, Android/Gradle)
- **Intelligent File Filtering**: Respects `.gitignore` files and provides custom include/exclude patterns
- **Multiple Output Modes**: Single consolidated file, split by project type, or copy to clipboard
- **AI-Ready Output**: Optimized formatting for AI assistants with token estimation
- **Binary File Detection**: Automatically skips binary files unless explicitly included
- **Cross-Platform**: Works on Windows, macOS, and Linux
- **Fast & Efficient**: Uses glob patterns and parallel processing for large codebases

## 🚀 Quick Start

### Installation

#### Option 1: Install as Global Tool (Recommended)
```bash
# Build and install the global tool
dotnet pack src/Codebase-Consolidator -c Release
dotnet tool install --global --add-source ./src/Codebase-Consolidator/nupkg Codebase-Consolidator

# Use anywhere in your system
consolidate --help
consolidate /path/to/your/project
```

#### Option 2: Download Release
Download the latest release from the [Releases page](https://github.com/your-username/codebase-consolidator/releases) and add it to your PATH.

#### Option 3: Build from Source
```bash
git clone https://github.com/your-username/codebase-consolidator.git
cd codebase-consolidator
dotnet build --configuration Release
```

### Basic Usage

```bash
# Consolidate current directory
consolidate

# Consolidate specific directory
consolidate C:\Projects\MyWebApp

# Save to specific file
consolidate . -o my-project.txt

# Copy to clipboard
consolidate . --clipboard

# Dry run (see what files would be included)
consolidate . --dry-run
```

## 📖 Usage Examples

### Single Project Consolidation
```bash
# Basic consolidation
consolidate C:\Projects\MyApp

# With custom exclusions
consolidate . --exclude "**/*.log" --exclude "**/temp/*"

# Include specific files even if gitignored
consolidate . --include "**/appsettings.json" --exclude "**/bin/**"

# Include binary files
consolidate . --include-binary
```

### Multi-Project Workflows
```bash
# Split by C# projects
consolidate . --split-by csproj

# Split by Node.js projects
consolidate . --split-by package.json

# Split by Maven projects
consolidate . --split-by pom.xml

# Split by PHP Composer projects
consolidate . --split-by composer.json

# Split by Python projects
consolidate . --split-by pyproject.toml

# Split by Android/Gradle projects
consolidate . --split-by build.gradle
```

### AI Assistant Integration
```bash
# Copy to clipboard with token estimation
consolidate . --clipboard --token-model gpt-4

# Estimate tokens for different models
consolidate . --token-model gpt-4o
consolidate . --token-model claude-4-sonnet
consolidate . --token-model gemini-2.5-pro
```

## 🛠️ Supported Project Types

| Project Type | Detection File | Strategy |
|--------------|----------------|----------|
| **C# / .NET** | `*.csproj` | Includes C#, config, and project files |
| **Node.js** | `package.json` | Includes JavaScript, TypeScript, config files |
| **Java Maven** | `pom.xml` | Includes Java source and Maven configuration |
| **PHP Composer** | `composer.json` | Includes PHP source and Composer files |
| **Python** | `pyproject.toml` | Includes Python source and configuration |
| **Android/Gradle** | `build.gradle` | Includes Java/Kotlin, Android resources, Gradle config |

## 📋 Command Reference

### Arguments
- `[PROJECT_ROOT]` - Root directory to consolidate (defaults to current directory)

### Options
| Option | Description |
|--------|-------------|
| `-o, --output <FILE>` | Output file path |
| `-e, --exclude <PATTERN>` | Additional exclusion patterns (can be used multiple times) |
| `-i, --include <PATTERN>` | Force include patterns (can be used multiple times) |
| `--include-binary` | Include binary files |
| `--dry-run` | List files without creating output |
| `-c, --clipboard` | Copy to clipboard instead of file |
| `--token-model <MODEL>` | Model for token estimation (default: gpt-4) |
| `--split-by <TYPE>` | Split output by project type (`csproj`, `package.json`, `pom.xml`, `composer.json`, `pyproject.toml`, `build.gradle`) |

### Supported Token Models
- `gpt-4`, `gpt-4o`, `gpt-4-turbo`
- `gpt-3.5-turbo`
- `claude-4-sonnet`, `claude-3-opus`, `claude-3-sonnet`
- `gemini-2.5-pro`, `gemini-1.5-pro`

## 📁 File Filtering

### Default Exclusions
The tool automatically excludes common build artifacts and temporary files:
- `bin/`, `obj/`, `node_modules/`, `target/`, `vendor/`
- `*.exe`, `*.dll`, `*.so`, `*.dylib`
- `.git/`, `.vs/`, `.vscode/`
- Log files, temporary files, and OS-specific files

### Custom Patterns
Use glob patterns for fine-grained control:

```bash
# Exclude all log files
consolidate . --exclude "**/*.log"

# Exclude specific directories
consolidate . --exclude "**/bin/**" --exclude "**/obj/**"

# Include specific config files even if gitignored
consolidate . --include "**/appsettings.production.json"

# Complex pattern combinations
consolidate . --exclude "**/*.min.js" --include "**/important.min.js"
```

## 🏗️ Project Structure

```
Codebase-Consolidator/
├── src/
│   └── Codebase-Consolidator/           # Main application
│       ├── ConsolidateCommand.cs        # CLI command implementation
│       ├── GitIgnoreParser.cs           # .gitignore parsing logic
│       ├── Program.cs                   # Application entry point
│       └── ProjectDiscoveryStrategies.cs # Project type detection
├── tests/
│   └── Codebase-Consolidator.Tests/     # Comprehensive test suite
│       ├── BasicTests.cs                # Core functionality tests
│       ├── ProjectDiscoveryTests.cs     # Project strategy tests
│       ├── GitIgnoreParserTests.cs      # GitIgnore parsing tests
│       ├── EdgeCaseTests.cs             # Edge cases and error handling
│       ├── IntegrationTests.cs          # End-to-end tests
│       └── TestData/                    # Sample files for testing
└── docs/                                # Documentation
```

## 🧪 Testing

The project includes a comprehensive test suite with 54+ tests covering:

- Core functionality and component integration
- All project discovery strategies (C#, Node.js, Maven, PHP, Python, Android/Gradle)
- GitIgnore pattern parsing and file exclusion
- Edge cases and error conditions
- End-to-end CLI workflows

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal

# Run specific test category
dotnet test --filter "ProjectDiscoveryTests"
```

## 🤝 Contributing

We welcome contributions! Here's how to get started:

1. **Fork the repository**
2. **Create a feature branch**: `git checkout -b feature/amazing-feature`
3. **Write tests** for your changes
4. **Ensure all tests pass**: `dotnet test`
5. **Commit your changes**: `git commit -m 'Add amazing feature'`
6. **Push to the branch**: `git push origin feature/amazing-feature`
7. **Open a Pull Request**

### Development Setup

1. Clone the repository
2. Ensure you have .NET 9.0 SDK installed
3. Run `dotnet restore` to restore dependencies
4. Run `dotnet build` to build the project
5. Run `dotnet test` to execute the test suite

## 📝 Output Format

The consolidated output includes:

```
=== PROJECT SUMMARY ===
Project Type: [Detected Type]
Root Directory: [Path]
Total Files: [Count]
Total Size: [Size]
Estimated Tokens: [Count] ([Model])

=== FILES ===

--- [filepath] ---
[file contents]

--- [filepath] ---
[file contents]
```

When using `--split-by`, each project gets its own file with the naming pattern:
`[output-base]-[project-name].[ext]`

## 📊 Token Estimation

The tool provides accurate token estimates for popular AI models to help you stay within context limits:

- Displays total estimated tokens
- Shows model-specific limits
- Warns when approaching limits
- Supports multiple tokenizer types

## 🐛 Troubleshooting

### Common Issues

**Issue**: "Project root directory not found"
- **Solution**: Ensure the path exists and you have read permissions

**Issue**: Binary files being included
- **Solution**: Use `--exclude "**/*.exe"` or check your `.gitignore`

**Issue**: Important files being excluded
- **Solution**: Use `--include` patterns to force inclusion

**Issue**: Token count too high
- **Solution**: Use `--exclude` patterns to reduce scope or `--split-by` for multiple files

### Logging

The tool creates detailed logs at `consolidator-log-YYYYMMDD.txt` for debugging.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Built with [Spectre.Console](https://spectreconsole.net/) for beautiful CLI experiences
- Uses [TiktokenSharp](https://github.com/aiqinxuancai/TiktokenSharp) for accurate token estimation
- Powered by [Microsoft.Extensions.FileSystemGlobbing](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.filesystemglobbing) for pattern matching

---

**Made with ❤️ for developers who work with AI assistants**

🔗 **Links**: [Documentation](docs/) | [Issues](https://github.com/your-username/codebase-consolidator/issues) | [Releases](https://github.com/your-username/codebase-consolidator/releases)
