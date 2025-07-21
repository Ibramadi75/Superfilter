![pre-alpha](https://img.shields.io/badge/status-Available_soon_🚧-ff69b4?style=for-the-badge&logoColor=white&label=WIP)
[![NuGet](https://img.shields.io/nuget/v/Superfilter?style=for-the-badge)](https://www.nuget.org/packages/Superfilter/)
[![GitHub](https://img.shields.io/badge/GitHub-Repository-blue?style=for-the-badge&logo=github)](https://github.com/Ibramadi75/Superfilter)

# Superfilter

Superfilter is a lightweight C# .NET 9.0 library for applying dynamic filtering and sorting on `IQueryable` sources. It maps textual filter criteria to strongly typed expressions, making it easy to expose flexible query capabilities in web APIs or other data-driven applications.

## Features

- **🚀 Fluent IQueryable Extensions** - Natural integration with LINQ queries
- Map filter keys to entity properties with lambda selectors and type inference
- Supports nested navigation properties (e.g., `x => x.Car.Brand.Name`)
- Validates required filters with configurable error handling
- Built‑in operators: `Equals`, `LessThan`, `GreaterThan`, `StartsWith`, `Contains`, `IsEqualToYear`, `IsEqualToYearAndMonth`, `IsEqualToFullDate`
- Integrated sorting functionality
- Works seamlessly with Entity Framework and LINQ-to-Objects
- Type-safe expression building with proper type inference
- **IntelliSense support** with compile-time type checking

## Getting Started

### 🚀 Fluent IQueryable Extensions

```csharp
using Superfilter;

// In a controller or service method
[HttpPost("search")]
public async Task<IActionResult> SearchUsers([FromBody] UserSearchRequest request)
{
    // Apply filters using fluent IQueryable extensions
    var result = await _context.Users
        .WithSuperfilter()                               // Start fluent configuration
        .MapProperty(u => u.Id)                          // IntelliSense support
        .MapProperty(u => u.Car.Brand.Name)              // Navigation properties work naturally
        .MapProperty("name", u => u.Name)                // Explicit key usage
        .MapRequiredProperty(u => u.MoneyAmount)         // Require this property to be included in filters
        .WithFilters(request.Filters)                    // Apply dynamic filters - returns filtered IQueryable
        .ToListAsync();
    
    return Ok(result);
}
```

### With Sorting

```csharp
// Apply both filters and sorting
var result = await _context.Users
    .WithSuperfilter()
    .MapProperty(u => u.Name)
    .MapProperty(u => u.Age)
    .MapProperty(u => u.MoneyAmount)
    .WithFilters(request.Filters)                        // Apply filters
    .ApplySorting(request.Sorts)                         // Apply sorting
    .ToListAsync();
```

## API Reference

### Core Extension Methods

| Method | Description |
|--------|-------------|
| `.WithSuperfilter()` | Starts fluent configuration chain for IQueryable |
| `MapProperty<TProperty>(selector)` | Maps property with auto-generated key |
| `MapProperty<TProperty>(key, selector)` | Maps property with explicit key |
| `MapRequiredProperty<TProperty>(selector)` | Maps required property with auto-generated key |
| `MapRequiredProperty<TProperty>(key, selector)` | Maps required property with explicit key |
| `WithFilters(IHasFilters)` | Applies filters and returns filtered IQueryable |
| `AddStaticFilter(field, operator, value)` | Adds a static filter |
| `WithErrorStrategy(OnErrorStrategy)` | Sets error handling strategy |

### Property Mapping Examples

```csharp
// MapProperty handles all types automatically with type inference
var result = _context.Users
    .WithSuperfilter()
    .MapProperty(u => u.Name)                    // string - auto key: "User.Name"
    .MapProperty(u => u.Id)                      // int - auto key: "User.Id"
    .MapProperty(u => u.BornDate)                // DateTime? - auto key: "User.BornDate"
    .MapProperty(u => u.MoneyAmount)             // int - auto key: "User.MoneyAmount"
    .MapProperty(u => u.IsActive)                // bool - auto key: "User.IsActive"
    .MapProperty(u => u.Car.Brand.Name)          // nested string - auto key: "User.Car.Brand.Name"
    .MapProperty("customKey", u => u.Email)      // explicit key
    .WithFilters(request.Filters)
    .ToList();
```

### Error Handling

```csharp
// Configure error handling strategy
var result = _context.Users
    .WithSuperfilter()
    .WithErrorStrategy(OnErrorStrategy.Ignore)   // Or OnErrorStrategy.ThrowException
    .MapProperty(u => u.Name)
    .WithFilters(request.Filters)
    .ToList();
```

### Static Filters

```csharp
// Add static filters (applied in addition to dynamic filters)
var result = _context.Users
    .WithSuperfilter()
    .MapProperty(u => u.Name)
    .MapProperty(u => u.IsActive)
    .AddStaticFilter("User.IsActive", Operator.Equals, "true")  // Always filter active users
    .WithFilters(request.Filters)                               // Plus dynamic filters from client
    .ToList();
```

## Supported Operators

| Operator | Description | Example |
|----------|-------------|---------|
| `Equals` | Exact match | `name = "John"` |
| `Contains` | String contains | `name LIKE "%John%"` |
| `StartsWith` | String starts with | `name LIKE "John%"` |
| `LessThan` | Numeric/Date less than | `age < 30` |
| `GreaterThan` | Numeric/Date greater than | `age > 18` |
| `IsEqualToYear` | Date year equals | `YEAR(birthDate) = 1990` |
| `IsEqualToYearAndMonth` | Date year and month equals | `YEAR(birthDate) = 1990 AND MONTH(birthDate) = 5` |
| `IsEqualToFullDate` | Full date equals | `DATE(birthDate) = '1990-05-15'` |

## Note on Property Mapping

The `MapProperty` method is available in two forms:

1. `MapProperty(key, selector)` - Explicit key definition for the property mapping
2. `MapProperty(selector)` - Auto-generated key based on property path

**Security Note:** When using auto-generated keys (`MapProperty(selector)`), consider whether exposing your data model schema to the frontend is acceptable for your use case. This approach might reveal internal details of your data model structure.

## Installation

```bash
dotnet add package Superfilter --version 0.1.6-alpha
```

## Links

- 📦 **[NuGet Package](https://www.nuget.org/packages/Superfilter/)** - Install from NuGet.org
- 🐙 **[GitHub Repository](https://github.com/Ibramadi75/Superfilter)** - Source code and issues
- 📖 **[Ibradev.fr/Superfilter](https://ibradev.fr/superfilter)** - Project's page

## Project Structure

```
Superfilter/
├── SuperFilter/           # Core library implementation
│   ├── Extensions/        # IQueryable extensions
│   ├── Constants/         # Operator definitions
│   └── ExpressionBuilders/# Type-specific expression builders
├── Database/              # EF Core context and domain models
└── Tests/                 # Comprehensive test suite
```

## Running Tests

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal

# Exclude PostgreSQL integration tests (require Docker)
dotnet test --filter "FullyQualifiedName!~PostgreSqlIntegrationTests"
```

## Key Benefits

- ✅ **Fluent Integration** with existing IQueryable workflows
- ✅ **Type Safety** with compile-time checking and IntelliSense support
- ✅ **Zero Configuration** - works out of the box with sensible defaults
- ✅ **Natural Navigation Properties** support without complex setup
- ✅ **Flexible Filtering** - combine static and dynamic filters seamlessly
- ✅ **Entity Framework Ready** - optimized for EF Core query generation

## License

This project is licensed under the [MIT License](LICENSE).