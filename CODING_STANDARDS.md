# Coding Standards

## Naming Conventions
- **Files**: PascalCase, descriptive names (e.g., `UserService.cs`)
- **Interfaces**: Prefix with `I` (e.g., `IUserService`)
- **Classes**: PascalCase, descriptive (e.g., `UserService`)
- **Namespaces**: One-line format preferred over block format
  ```csharp
  namespace MyProject.Services;
  ```

## C# Language Standards

### Program Structure
- Use explicit `Main` method instead of top-level statements
  ```csharp
  public class Program
  {
      public static void Main(string[] args)
      {
          // Application setup
      }
  }
  ```

### Properties and Fields
- Always use properties instead of fields
- Use auto-implemented properties with `{ get; set; }`
- For public properties, use `{ get; private set; }` when possible
- Initialize collections with `= new()` or `Array.Empty<T>()`
- Use `null!` for dependency injection properties that will be initialized

### Method Signatures
- Use `async Task<T>` for asynchronous methods
- Suffix async methods with `Async`
- Use explicit parameter types

### Error Handling
- Use try-catch blocks in all public methods
- Log exceptions to console with context
- Use nullable returns with null coalescing (`?? new()`)

### Code Style
- Use `this.` prefix for instance members
- Use `is true`/`is false` for boolean comparisons
- Use `is null`/`is not null` for null checks
- Use pattern matching where appropriate
- Never use primary constructors - use traditional constructor syntax
- Never use single-line if statements, loops, or other control structures - always use braces

## Blazor Component Standards

### Component Structure
- Separate markup (`.razor`) from code-behind (`.razor.cs`)
- Use `partial class` for code-behind components

### Dependency Injection
- Use `[Inject]` attribute for service dependencies
- Initialize injected properties with `= null!`
- Register services in `Program.cs` with appropriate lifetimes

### Error Handling in Components
- Implement loading states with boolean flags
- Handle errors with string properties
- Use try-catch-finally pattern for async operations

## Service Standards

### Service Implementation
- Implement shared interfaces for consistent contracts
- Use dependency injection for all external dependencies
- Handle exceptions gracefully with proper logging

## Configuration and Dependencies

### Service Registration
- Register services with appropriate lifetimes (Scoped, Singleton, Transient)
- Use `IConfiguration` for application settings
- Implement proper HTTP client configuration

### Environment Configuration
- Use `appsettings.json` for configuration
- Implement different settings for Development/Production
- Never commit secrets to source control
- Use environment variables for sensitive data

## Documentation and Comments

### Code Documentation
- Document complex business logic
- Note temporary or deprecated methods
- Use region blocks for logical grouping
- Provide descriptive error messages with context