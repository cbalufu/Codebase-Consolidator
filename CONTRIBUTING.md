# Contributing to Codebase Consolidator

Thank you for your interest in contributing to Codebase Consolidator! This document provides guidelines and information for contributors.

## 🚀 Getting Started

### Prerequisites
- .NET 9.0 SDK or later
- Git
- A code editor (Visual Studio, VS Code, JetBrains Rider, etc.)

### Setup Development Environment

1. **Fork and clone the repository**
   ```bash
   git clone https://github.com/your-username/codebase-consolidator.git
   cd codebase-consolidator
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the project**
   ```bash
   dotnet build
   ```

4. **Run tests to ensure everything works**
   ```bash
   dotnet test
   ```

5. **Test the CLI locally**
   ```bash
   cd src/Codebase-Consolidator
   dotnet run -- --help
   ```

## 🧪 Testing

We maintain a comprehensive test suite with 40+ tests. All contributions should include appropriate tests.

### Running Tests
```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Run specific test class
dotnet test --filter "GitIgnoreParserTests"

# Run tests with coverage (if you have the tools installed)
dotnet test --collect:"XPlat Code Coverage"
```

### Test Structure
- `BasicTests.cs` - Core functionality tests
- `ProjectDiscoveryTests.cs` - Project detection strategy tests
- `GitIgnoreParserTests.cs` - .gitignore parsing tests
- `EdgeCaseTests.cs` - Edge cases and error handling
- `IntegrationTests.cs` - End-to-end workflow tests

### Writing Tests
- Follow the Arrange-Act-Assert pattern
- Use descriptive test method names
- Include both positive and negative test cases
- Test edge cases and error conditions
- Use temporary directories for file system tests
- Ensure proper cleanup in `finally` blocks

## 📝 Code Style and Standards

### C# Coding Standards
- Follow Microsoft's C# coding conventions
- Use meaningful variable and method names
- Include XML documentation for public APIs
- Use `var` for local variables when the type is obvious
- Prefer explicit access modifiers

### File Organization
```
src/Codebase-Consolidator/
├── Program.cs                    # Application entry point
├── ConsolidateCommand.cs         # Main CLI command logic
├── ConsolidateSettings.cs        # Command-line options
├── GitIgnoreParser.cs            # .gitignore file parsing
└── ProjectDiscoveryStrategies.cs # Project type detection
```

### Dependencies
- Keep dependencies minimal and well-justified
- Prefer Microsoft.Extensions.* packages when possible
- Document any new dependencies in pull requests

## 🐛 Bug Reports

When reporting bugs, please include:

1. **Environment Information**
   - Operating System and version
   - .NET version
   - Tool version

2. **Reproduction Steps**
   - Detailed steps to reproduce the issue
   - Sample project structure if relevant
   - Command-line arguments used

3. **Expected vs Actual Behavior**
   - What you expected to happen
   - What actually happened
   - Any error messages or logs

4. **Additional Context**
   - Screenshots if applicable
   - Log files (`consolidator-log-YYYYMMDD.txt`)

## ✨ Feature Requests

For new features:

1. **Check existing issues** to avoid duplicates
2. **Describe the use case** - what problem does this solve?
3. **Provide examples** of how the feature would be used
4. **Consider backward compatibility** and impact on existing users

## 🔧 Making Changes

### Workflow
1. **Create a feature branch** from `main`
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes**
   - Write code following the established patterns
   - Add or update tests as needed
   - Update documentation if required

3. **Ensure quality**
   ```bash
   # Build and test
   dotnet build
   dotnet test
   
   # Test CLI functionality manually
   cd src/Codebase-Consolidator
   dotnet run -- . --dry-run
   ```

4. **Commit with descriptive messages**
   ```bash
   git add .
   git commit -m "Add support for Rust project detection"
   ```

5. **Push and create a pull request**
   ```bash
   git push origin feature/your-feature-name
   ```

### Pull Request Guidelines

#### Before Submitting
- [ ] All tests pass (`dotnet test`)
- [ ] Code builds without warnings (`dotnet build`)
- [ ] New functionality includes tests
- [ ] Documentation updated if needed
- [ ] Commit messages are clear and descriptive

#### PR Description Should Include
- Summary of changes
- Motivation and context
- Type of change (bug fix, feature, refactoring, etc.)
- Testing performed
- Screenshots (if UI changes)

## 🏗️ Architecture Guidelines

### Adding New Project Types
To add support for a new project type:

1. **Add strategy class** in `ProjectDiscoveryStrategies.cs`
   ```csharp
   public class NewProjectStrategy : IProjectDiscoveryStrategy
   {
       public string StrategyName => "new-project-file";
       // Implement interface methods
   }
   ```

2. **Register in discovery** 
   Add to the strategies list in `ConsolidateCommand.cs`

3. **Add comprehensive tests**
   - Basic project detection
   - Project name extraction
   - File inclusion logic
   - Edge cases (malformed files, missing files)

4. **Update documentation**
   - Add to README.md supported project types table
   - Add example usage
   - Update help text if needed

### Extending GitIgnore Support
For .gitignore enhancements:

1. **Modify `GitIgnoreParser.cs`**
2. **Add test cases** in `GitIgnoreParserTests.cs`
3. **Test with real .gitignore files** from popular projects

### CLI Enhancements
For new command-line options:

1. **Add to `ConsolidateSettings.cs`**
2. **Implement in `ConsolidateCommand.cs`**
3. **Add integration tests**
4. **Update help examples**

## 📚 Documentation

### Code Documentation
- Use XML documentation comments for public APIs
- Include examples in documentation when helpful
- Document complex algorithms or business logic

### User Documentation
- Update README.md for new features
- Add examples to QUICK_START.md
- Update command reference

## 🤝 Community Guidelines

### Be Respectful
- Use welcoming and inclusive language
- Be respectful of different viewpoints and experiences
- Focus on what is best for the community
- Show empathy towards other community members

### Communication
- Use clear, concise language in issues and PRs
- Provide constructive feedback
- Ask for clarification when needed
- Be patient with new contributors

## 📋 Issue Labels

We use these labels to organize issues:

- `bug` - Something isn't working
- `enhancement` - New feature or request
- `documentation` - Improvements or additions to documentation
- `good first issue` - Good for newcomers
- `help wanted` - Extra attention is needed
- `question` - Further information is requested

## ⚡ Performance Considerations

When contributing:

- **Profile before optimizing** - measure performance impact
- **Consider memory usage** especially for large codebases
- **Use async/await** for I/O operations
- **Minimize allocations** in hot paths
- **Test with large projects** to ensure scalability

## 🎉 Recognition

Contributors will be:
- Added to the contributors list
- Credited in release notes for significant contributions
- Welcomed into the community with gratitude

Thank you for contributing to make Codebase Consolidator better for everyone! 🚀
