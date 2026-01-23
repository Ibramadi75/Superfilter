![stable](https://img.shields.io/badge/status-Stable_1.0.1-green?style=for-the-badge)
[![NuGet](https://img.shields.io/nuget/v/Superfilter?style=for-the-badge)](https://www.nuget.org/packages/Superfilter/)
[![GitHub](https://img.shields.io/badge/GitHub-Repository-blue?style=for-the-badge&logo=github)](https://github.com/Ibramadi75/Superfilter)

# Superfilter

Superfilter is a lightweight C# library for applying dynamic filtering, sorting, and pagination on `IQueryable` sources. It
maps textual filter criteria to strongly typed expressions, making it easy to expose flexible query capabilities in web
APIs or other data-driven applications.

**🎯 Framework Support:** .NET 8.0 and .NET 9.0

## Features

- **🚀 Fluent IQueryable Extensions** - Natural integration with LINQ queries
- **📄 Built-in Pagination** - Page-based and offset-based pagination support
- Map filter keys to entity properties with lambda selectors and type inference
- Supports nested navigation properties (e.g., `x => x.Car.Brand.Name`)
- Validates required filters with configurable error handling
- Built‑in operators: `Equals`, `LessThan`, `GreaterThan`, `StartsWith`, `Contains`, `IsEqualToYear`,
  `IsEqualToYearAndMonth`, `IsEqualToFullDate`
- Integrated sorting functionality
- Works seamlessly with Entity Framework and LINQ-to-Objects
- Type-safe expression building with proper type inference
- **IntelliSense support** with compile-time type checking

## Getting Started

### Request DTO Setup

```csharp
// Your request DTO should implement the required interfaces
public class UserSearchRequest : IHasFilters, IHasPagination
{
    public List<FilterCriterion> Filters { get; set; } = new();
    public Pagination Pagination { get; set; } = new(1, 10);
}
```

### 🚀 Basic Usage (Recommended Pattern)

```csharp
using Superfilter;

[HttpPost("search")]
public async Task<IActionResult> SearchUsers([FromBody] UserSearchRequest request)
{
    var result = await _context.Users
        .WithSuperfilter(request)                    // Pass request once - stores filters & pagination
        .MapProperty("name", u => u.Name)            // Map filterable properties
        .MapProperty("age", u => u.Age)
        .MapProperty(u => u.Car.Brand.Name)          // Navigation properties with auto-generated key
        .ApplyFilters()                              // Apply filters and sorts
        .ApplyPagination()                           // Apply pagination (Skip/Take)
        .ToListAsync();

    return Ok(result);
}
```

### Without Pagination

```csharp
// If you don't need pagination, just call ApplyFilters()
var result = await _context.Users
    .WithSuperfilter(request)
    .MapProperty("name", u => u.Name)
    .ApplyFilters()                                  // Returns FilteredQueryable<T>
    .ToListAsync();                                  // Works directly - no pagination applied
```

### With Sorting

```csharp
// Sorting is automatically applied from request.Sorters if your DTO implements IHasSorts
public class UserSearchRequest : IHasFilters, IHasSorts, IHasPagination
{
    public List<FilterCriterion> Filters { get; set; } = new();
    public List<SortCriterion> Sorters { get; set; } = new();
    public Pagination Pagination { get; set; } = new(1, 10);
}

var result = await _context.Users
    .WithSuperfilter(request)
    .MapProperty("name", u => u.Name)
    .MapProperty("age", u => u.Age)
    .ApplyFilters()                                  // Applies both filters AND sorts
    .ApplyPagination()
    .ToListAsync();
```

### Offset-based Pagination

```csharp
// Use ApplyOffsetPagination for skip/take style pagination
var result = await _context.Users
    .WithSuperfilter(request)
    .MapProperty("name", u => u.Name)
    .ApplyFilters()
    .ApplyOffsetPagination(skip: 20, take: 10)       // Skip 20, take 10
    .ToListAsync();
```

### Alternative Pattern (Without Request in WithSuperfilter)

```csharp
// You can also pass filters separately - useful when filters come from different sources
var result = await _context.Users
    .WithSuperfilter()                               // No request passed
    .MapProperty("name", u => u.Name)
    .MapProperty("age", u => u.Age)
    .WithFilters(filtersFromOneSource)               // Pass filters explicitly
    .ApplyFilters()
    .ApplyPagination(paginationFromAnotherSource)    // Pass pagination explicitly
    .ToListAsync();
```

## API Reference

### Core Extension Methods

| Method                                          | Description                                              |
|-------------------------------------------------|----------------------------------------------------------|
| `.WithSuperfilter(request)`                     | Starts fluent chain with request (recommended)           |
| `.WithSuperfilter()`                            | Starts fluent chain without request                      |
| `MapProperty<TProperty>(selector)`              | Maps property with auto-generated key                    |
| `MapProperty<TProperty>(key, selector)`         | Maps property with explicit key                          |
| `MapRequiredProperty<TProperty>(selector)`      | Maps required property with auto-generated key           |
| `ApplyFilters()`                                | Applies filters/sorts, returns `FilteredQueryable<T>`    |
| `ApplyPagination()`                             | Applies pagination from stored request                   |
| `ApplyPagination(IHasPagination)`               | Applies pagination from provided object                  |
| `ApplyPagination(pageNumber, pageSize)`         | Applies pagination with explicit values                  |
| `ApplyOffsetPagination(skip, take)`             | Applies offset-based pagination                          |
| `ApplyFiltersAndPagination()`                   | Shortcut for `ApplyFilters().ApplyPagination()`          |
| `AddStaticFilter(field, operator, value)`       | Adds a static filter                                     |
| `WithErrorStrategy(OnErrorStrategy)`            | Sets error handling strategy                             |

### Property Mapping Examples

```csharp
var result = _context.Users
    .WithSuperfilter(request)
    .MapProperty("name", u => u.Name)                // string - explicit key
    .MapProperty(u => u.Id)                          // int - auto key: "User.Id"
    .MapProperty(u => u.BornDate)                    // DateTime? - auto key: "User.BornDate"
    .MapProperty(u => u.Car.Brand.Name)              // nested - auto key: "User.Car.Brand.Name"
    .MapRequiredProperty("amount", u => u.MoneyAmount) // required filter
    .ApplyFilters()
    .ToList();
```

### Static Filters

```csharp
// Add static filters (applied in addition to dynamic filters)
var result = _context.Users
    .WithSuperfilter(request)
    .MapProperty("name", u => u.Name)
    .MapProperty("isActive", u => u.IsActive)
    .AddStaticFilter("isActive", Operator.Equals, "true")  // Always filter active users
    .ApplyFilters()
    .ApplyPagination()
    .ToList();
```

### Error Handling

```csharp
var result = _context.Users
    .WithSuperfilter(request)
    .WithErrorStrategy(OnErrorStrategy.Ignore)       // Or OnErrorStrategy.ThrowException
    .MapProperty("name", u => u.Name)
    .ApplyFilters()
    .ToList();
```

## Supported Operators

| Operator                | Description                | Example                                           |
|-------------------------|----------------------------|---------------------------------------------------|
| `Equals`                | Exact match                | `name = "John"`                                   |
| `Contains`              | String contains            | `name LIKE "%John%"`                              |
| `StartsWith`            | String starts with         | `name LIKE "John%"`                               |
| `LessThan`              | Numeric/Date less than     | `age < 30`                                        |
| `GreaterThan`           | Numeric/Date greater than  | `age > 18`                                        |
| `IsEqualToYear`         | Date year equals           | `YEAR(birthDate) = 1990`                          |
| `IsEqualToYearAndMonth` | Date year and month equals | `YEAR(birthDate) = 1990 AND MONTH(birthDate) = 5` |
| `IsEqualToFullDate`     | Full date equals           | `DATE(birthDate) = '1990-05-15'`                  |

## Note on Property Mapping

The `MapProperty` method is available in two forms:

1. `MapProperty(key, selector)` - Explicit key definition for the property mapping
2. `MapProperty(selector)` - Auto-generated key based on property path

**Security Note:** When using auto-generated keys (`MapProperty(selector)`), consider whether exposing your data model
schema to the frontend is acceptable for your use case.

### Benchmarks show better results with Superfilter
<img width="1006" height="251" alt="image" src="https://github.com/user-attachments/assets/0dc5d072-72f1-4734-b32c-c132ffab9c02" />

## Installation

```bash
dotnet add package Superfilter --version 1.0.1
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
- ✅ **Async Support** - Full compatibility with `ToListAsync()` and other async methods

## License

This project is licensed under the [MIT License](LICENSE).
