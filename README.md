![pre-alpha](https://img.shields.io/badge/status-Available_soon_🚧-ff69b4?style=for-the-badge&logoColor=white&label=WIP)
[![NuGet](https://img.shields.io/nuget/v/Superfilter?style=for-the-badge)](https://www.nuget.org/packages/Superfilter/)
[![GitHub](https://img.shields.io/badge/GitHub-Repository-blue?style=for-the-badge&logo=github)](https://github.com/Ibramadi75/Superfilter)

# Superfilter

Superfilter is a lightweight C# .NET 9.0 library for applying dynamic filtering and sorting on `IQueryable` sources. It maps textual filter criteria to strongly typed expressions, making it easy to expose flexible query capabilities in web APIs or other data-driven applications.

## Features

- **🚀 New: Fluent ConfigurationBuilder API** - No more manual casting required!
- Map filter keys to entity properties through `FieldConfiguration` with lambda selectors
- Supports nested navigation properties (e.g., `x => x.Car.Brand.Name`)
- Validates required filters with configurable error handling
- Built‑in operators: `Equals`, `LessThan`, `GreaterThan`, `StartsWith`, `Contains`, `IsEqualToYear`, `IsEqualToYearAndMonth`, `IsEqualToFullDate`
- Fluent API for sorting via `ApplySorting` extension method
- Works with Entity Framework and LINQ-to-Objects
- Type-safe expression building with proper type inference
- **IntelliSense support** with compile-time type checking

## Getting Started

### 🚀 Simplified API (One Instance = One Type)

```csharp
using Superfilter;

// In a controller or service method
[HttpPost("search")]
public async Task<IActionResult> SearchUsers([FromBody] UserSearchRequest request)
{
    // Create ready-to-use Superfilter instance with type-safe property mappings
    var superfilter = SuperfilterBuilder.For<User>()
        .MapProperty(u => u.Id)                          // IntelliSense support
        .MapProperty(u => u.Car.Brand.Name)              // Navigation properties work naturally
        .MapProperty("name", u => u.Name)                // Explicit key usage; prevent exposes the property path as the key
        .MapRequiredProperty(u => u.MoneyAmount)         // Require this property to be included in filters
        .WithFilters(request.Filters)                    // Dynamic filters from client
        .WithSorts(request.Sorts)                        // Dynamic sorts from client
        .Build();                                        // Ready-to-use instance!

    // Apply filters and sorting directly
    var query = _context.Users.AsQueryable();
    query = superfilter.ApplyConfiguredFilters(query);
    query = query.ApplySorting(superfilter);
    
    return Ok(await query.ToListAsync());
}
```

## ConfigurationBuilder API Reference

### Core Methods

| Method | Description |
|--------|-------------|
| `SuperfilterBuilder.For<T>()` | Creates a new builder for entity type T |
| `MapProperty<TProperty>(key, selector, required)` | Maps any property with automatic type inference |
| `MapRequiredProperty<TProperty>(key, selector)` | Maps a required property |
| `Build()` | Creates a ready-to-use Superfilter instance |

### Property Mapping Examples

```csharp
// MapProperty handles all types automatically with type inference
var superfilter = SuperfilterBuilder.For<User>()
    .MapProperty(u => u.Name)                    // string
    .MapProperty(u => u.Id)                        // int  
    .MapProperty(u => u.BornDate)            // DateTime?
    .MapProperty(u => u.MoneyAmount)                     // int
    .MapProperty(u => u.IsActive)                        // bool
    .MapProperty(u => u.Car.Brand.Name)                  // nested string
    .Build();
```

### Dynamic Data Methods (From Client)

| Method | Description |
|--------|-------------|
| `WithFilters(IHasFilters)` | Set dynamic filters from client request |
| `WithSorts(IHasSorts)` | Set dynamic sorts from client request |

### Note on `MapProperty` Usage

The `MapProperty` method is available in two forms:

1. `MapProperty(key, selector)` - This allows you to explicitly define a key for the property mapping.
2. `MapProperty(selector)` - This alternative does not require a key and uses the property name directly.

**Important:** When using the second form (`MapProperty(selector)`), developers should carefully evaluate whether exposing the schema of their data model to the frontend is acceptable for their use case. This approach might inadvertently reveal sensitive or internal details of the data model.

## Installation

```bash
dotnet add package Superfilter --version 0.1.6-alpha
```

## Links

- 📦 **[NuGet Package](https://www.nuget.org/packages/Superfilter/)** - Install from NuGet.org
- 🐙 **[GitHub Repository](https://github.com/Ibramadi75/Superfilter)** - Source code and issues
- 📖 **[Ibradev.fr/Superfilter](https://ibradev.fr/superfilter)** - Project's page

## Project Layout

- `Superfilter/` – Core library implementation with partial class architecture
  - `Builder/` – ConfigurationBuilder API for fluent configuration
- `Database/` – EF Core context and domain models (User, Car, Brand, House, City)  
- `Tests/` – xUnit test suite covering filtering scenarios and edge cases

## Running Tests

```bash
dotnet test
```

## Key Benefits

- ✅ **No manual casting** required for PropertyMappings
- ✅ **Type safety** with compile-time checking
- ✅ **IntelliSense support** for better developer experience
- ✅ **No type redundancy** - specify type only once
- ✅ **One instance per entity type** for clean architecture
- ✅ **Natural navigation properties** support

## License

This project is licensed under the [MIT License](LICENSE).
