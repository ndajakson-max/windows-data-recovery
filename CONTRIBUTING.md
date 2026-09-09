# Contributing to Windows Data Recovery Tool

Thank you for your interest in contributing! This document provides guidelines and instructions for contributing.

## Code of Conduct

This project adheres to a code of conduct. By participating, you are expected to uphold this code.

## How to Contribute

### Reporting Bugs

1. **Use a clear, descriptive title** for the issue
2. **Describe the exact steps** which reproduce the problem
3. **Provide specific examples** to demonstrate the steps
4. **Describe the behavior you observed** and explain what's wrong
5. **Explain which behavior you expected** to see instead and why
6. **Include screenshots and animated GIFs** if possible
7. **Include your environment**: OS version, .NET version, etc.

### Suggesting Enhancements

1. **Use a clear, descriptive title** for the enhancement
2. **Provide a step-by-step description** of the suggested enhancement
3. **Provide specific examples** to demonstrate the steps
4. **Describe the current behavior** and explain the expected behavior
5. **Explain why this enhancement would be useful**

### Pull Requests

1. **Follow the C# coding style** used in the project
2. **Include appropriate test cases** for your changes
3. **Update documentation** as needed
4. **Keep commits atomic** with clear commit messages
5. **Reference any related issues** in your PR description

## Development Setup

1. Clone the repository
2. Install .NET 6.0 SDK or later
3. Open the solution in Visual Studio or your preferred IDE
4. Build the project: `dotnet build`
5. Run tests: `dotnet test`

## Project Structure

- `Core/` - Core recovery and scanning logic
- `Models/` - Data models
- `UI/` - WPF user interface
- `Utilities/` - Helper utilities
- `Tests/` - Unit tests

## Coding Standards

- Follow C# naming conventions (PascalCase for public members, camelCase for local variables)
- Use meaningful variable and function names
- Add XML documentation comments to public members
- Keep methods focused and reasonably sized
- Avoid code duplication

## License

By contributing, you agree that your contributions will be licensed under the MIT License.
