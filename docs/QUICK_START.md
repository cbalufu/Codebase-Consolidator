# Quick Start Guide

This guide will get you up and running with Codebase Consolidator in just a few minutes.

## Installation

### Windows
1. Download the latest release from GitHub
2. Extract to a folder (e.g., `C:\Tools\Codebase-Consolidator`)
3. Add the folder to your system PATH
4. Open a new command prompt and run `consolidate --help`

### macOS/Linux
1. Download the latest release
2. Extract to `/usr/local/bin` or another directory in your PATH
3. Make executable: `chmod +x consolidate`
4. Run `consolidate --help`

### Build from Source
```bash
git clone https://github.com/your-username/codebase-consolidator.git
cd codebase-consolidator
dotnet build --configuration Release
```

## Your First Consolidation

### Step 1: Basic Usage
Navigate to any project directory and run:
```bash
consolidate
```

This will:
- Scan the current directory for projects
- Respect your `.gitignore` files
- Create a consolidated output file
- Show token estimates

### Step 2: Review the Output
The tool creates a file like `consolidated-output-YYYYMMDD-HHMMSS.txt` containing:
- Project summary
- File listings
- Complete file contents
- Token estimation for AI models

### Step 3: Advanced Usage
Try these common scenarios:

```bash
# Copy to clipboard for AI assistants
consolidate . --clipboard

# Exclude additional files
consolidate . --exclude "**/*.log" --exclude "**/temp/*"

# Split by project type
consolidate . --split-by csproj

# Dry run to see what would be included
consolidate . --dry-run
```

## Common Workflows

### For AI Assistants
```bash
# Best for ChatGPT/Claude/Gemini
consolidate . --clipboard --token-model gpt-4
```

### For Code Reviews
```bash
# Create focused output
consolidate . --exclude "**/bin/**" --exclude "**/node_modules/**"
```

### For Documentation
```bash
# Include configuration files
consolidate . --include "**/appsettings*.json" --include "**/*.config"
```

### For Multi-Project Solutions
```bash
# Split by technology
consolidate . --split-by csproj     # Creates separate files for each C# project
consolidate . --split-by package.json  # Creates separate files for each Node.js project
```

## Tips and Tricks

1. **Use dry-run first**: Always run `--dry-run` on large projects to see what will be included
2. **Customize exclusions**: Add patterns to exclude build artifacts, logs, or temporary files
3. **Check token counts**: Use `--token-model` to estimate tokens for your target AI model
4. **Leverage gitignore**: The tool respects your existing `.gitignore` files automatically
5. **Split large projects**: Use `--split-by` to break large solutions into manageable chunks

## Troubleshooting

**Problem**: Output file is too large
- **Solution**: Use `--exclude` patterns to reduce scope or `--split-by` for multiple files

**Problem**: Missing important files
- **Solution**: Use `--include` patterns or check your `.gitignore` files

**Problem**: Binary files included
- **Solution**: The tool auto-detects binary files, but you can explicitly exclude with patterns

## Next Steps

- Read the full [README](../README.md) for comprehensive documentation
- Check out the [examples](#examples) section
- Explore the command-line options with `consolidate --help`
- Run the test suite to understand the codebase: `dotnet test`

## Examples

### Web Application
```bash
# Consolidate a web app, excluding build outputs
consolidate ./my-web-app --exclude "**/bin/**" --exclude "**/obj/**" --exclude "**/node_modules/**"
```

### API Project
```bash
# Focus on source code and configuration
consolidate ./my-api --include "**/*.cs" --include "**/*.json" --exclude "**/bin/**"
```

### Full Stack Solution
```bash
# Split into separate files for each technology
consolidate ./full-stack-solution --split-by csproj
consolidate ./full-stack-solution --split-by package.json
```

### Share with AI Assistant
```bash
# Optimize for AI context limits
consolidate . --clipboard --token-model gpt-4 --exclude "**/*.min.js" --exclude "**/*.map"
```
