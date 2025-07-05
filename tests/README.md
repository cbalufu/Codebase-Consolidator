# Codebase Consolidator Tests

This directory contains a comprehensive test suite for the Codebase Consolidator tool.

## Status

✅ **All 40 tests passing**

## Test Structure

```
tests/
├── Codebase-Consolidator.Tests/
│   ├── BasicTests.cs              # Core functionality tests (10 tests)
│   ├── ProjectDiscoveryTests.cs   # Project strategy tests (8 tests)
│   ├── GitIgnoreParserTests.cs    # GitIgnore parsing tests (5 tests)
│   ├── EdgeCaseTests.cs           # Edge cases and error handling (10 tests)
│   ├── IntegrationTests.cs        # End-to-end workflow tests (7 tests)
│   ├── TestData/                  # Sample files for testing
│   ├── TEST_SUITE_SUMMARY.md      # Detailed test documentation
│   └── appsettings.json           # Test configuration
```

### Key Test Classes
- **`BasicTests.cs`** - Basic functionality and component instantiation
- **`GitIgnoreParserTests.cs`** - .gitignore parsing and pattern matching
- **`ProjectDiscoveryTests.cs`** - All project discovery strategies
- **`EdgeCaseTests.cs`** - Error conditions and boundary testing  
- **`IntegrationTests.cs`** - End-to-end CLI and workflow testing

### Test Data
- **`TestData/`** - Sample project files (package.json, pom.xml, etc.)

## Running Tests

### Run all tests
```bash
# From solution root
cd d:\Tools\Codebase-Consolidator
dotnet test
```

### Run with detailed output
```bash
dotnet test --verbosity normal --logger "console;verbosity=detailed"
```

### Run specific test class
```bash
dotnet test --filter "GitIgnoreParserTests"
```

### Run from test directory
```bash
cd tests\Codebase-Consolidator.Tests
dotnet test
```

## Test Coverage

The test suite provides comprehensive coverage of:

✅ **GitIgnore Processing** 
- Pattern matching (wildcards, negation, comments)
- Directory and file exclusion patterns
- Nested .gitignore file handling
- Include pattern overrides
- Default exclusion patterns

✅ **Project Discovery**
- C# (.csproj) project detection
- Node.js (package.json) project detection  
- Maven (pom.xml) project detection
- PHP Composer (composer.json) project detection
- Python (pyproject.toml) project detection
- Android/Gradle (build.gradle) project detection
- Project name extraction from configuration files
- Fallback behavior for malformed files
- Multi-project scenarios

✅ **Command Execution & Integration**
- End-to-end CLI functionality
- Project splitting workflows
- Error handling for invalid inputs
- .gitignore integration in full workflow

✅ **Edge Cases & Error Handling**
- Empty configuration files
- Malformed JSON/XML files
- Non-existent files and directories
- Special characters in paths
- Very long file paths
- Symbolic links (where supported)

## Recent Improvements

### 🐛 Fixed Issues
- **GitIgnoreParser**: Fixed incorrect use of Microsoft.Extensions.FileSystemGlobbing.Matcher
- **NodeJS Strategy Test**: Fixed to expect project name from package.json instead of directory name

### 📈 Enhanced Coverage  
- **Before**: 10 basic tests (2 failing)
- **After**: 40 comprehensive tests (all passing)
- Added comprehensive edge case testing
- Added full integration testing
- Added error scenario testing
## Test Data Management

Tests create temporary directories and files that are automatically cleaned up after each test run. The test infrastructure:

- Creates isolated temporary directories for each test
- Automatically disposes of resources using `IDisposable`
- Uses realistic project structures and content
- Includes various file types (JSON, XML, text)
- Tests both valid and malformed configuration files

## Testing Framework & Dependencies

- **xUnit** - Primary testing framework
- **Microsoft.NET.Test.Sdk** - Test runner
- **Microsoft.Extensions.DependencyInjection** - Dependency injection support

## Documentation

See `Codebase-Consolidator.Tests/TEST_SUITE_SUMMARY.md` for detailed information about each test class and the specific improvements made.

## Contributing to Tests

When adding new features:

1. Add corresponding unit tests in the appropriate test class
2. Include integration tests for complex workflows  
3. Add edge case tests for error conditions
4. Add test data files if needed
5. Update documentation if test structure changes
6. Ensure all tests pass: `dotnet test`

## Future Enhancement Opportunities

Consider adding tests for:
- Performance with large codebases (stress testing)
- Cross-platform path handling variations
- Memory usage with very large file sets  
- Concurrency scenarios
- Token counting accuracy
- Clipboard operations
