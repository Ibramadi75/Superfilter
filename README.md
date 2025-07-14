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

### 🚀 New: Using ConfigurationBuilder

```csharp
using Superfilter;

// In a controller or service method
[HttpPost("search")]
public async Task<IActionResult> SearchUsers([FromBody] UserSearchRequest request)
{
    // 1. Create configuration with clean, type-safe property mappings
    var config = SuperfilterBuilder.For<User>()
        .MapRequiredProperty("id", u => u.Id)            // No casting required!
        .MapProperty("carBrandName", u => u.Car.Brand.Name)  // Type inference works for any type
        .MapProperty("name", u => u.Name)                // IntelliSense support
        .MapProperty("moneyAmount", u => u.MoneyAmount)  // Handles int, string, DateTime, etc.
        .WithFilters(request.Filters)                    // Dynamic filters from client
        .WithSorts(request.Sorts)                        // Dynamic sorts from client
        .Build();

    // 2. Use with Superfilter
    var superfilter = new Superfilter.Superfilter();
    superfilter.InitializeGlobalConfiguration(config);
    superfilter.InitializeFieldSelectors<User>();

    // 3. Apply to query
    var query = _context.Users.AsQueryable();
    query = superfilter.ApplyConfiguredFilters(query);
    query = query.ApplySorting(config);
    
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
| `Build()` | Creates the final GlobalConfiguration |

### Property Mapping Examples

```csharp
// MapProperty handles all types automatically with type inference
SuperfilterBuilder.For<User>()
    .MapProperty("name", u => u.Name)                    // string
    .MapProperty("id", u => u.Id)                        // int  
    .MapProperty("bornDate", u => u.BornDate)            // DateTime?
    .MapProperty("moneyAmount", u => u.MoneyAmount)      // int
    .MapProperty("isActive", u => u.IsActive)            // bool
    .MapProperty("carBrandName", u => u.Car.Brand.Name)  // nested string
```

### Dynamic Data Methods (From Client)

| Method | Description |
|--------|-------------|
| `WithFilters(IHasFilters)` | Set dynamic filters from client request |
| `WithSorts(IHasSorts)` | Set dynamic sorts from client request |

## Installation

```bash
dotnet add package Superfilter --version 0.1.4-alpha
```

## Links

- 📦 **[NuGet Package](https://www.nuget.org/packages/Superfilter/)** - Install from NuGet.org
- 🐙 **[GitHub Repository](https://github.com/Ibramadi75/Superfilter)** - Source code and issues
- 📖 **[ConfigurationBuilder Examples](CONFIGURATION_BUILDER_EXAMPLES.md)** - Comprehensive examples and use cases
- 🚀 **[Integration Guide](CONFIGURATION_BUILDER_INTEGRATION.md)** - Migration guide and best practices
- 📋 **[GitHub Issue](ISSUE_PROPERTY_MAPPING_CASTING.md)** - Background on the manual casting problem

## Project Layout

- `Superfilter/` – Core library implementation with partial class architecture
  - `Builder/` – **New:** ConfigurationBuilder API for fluent configuration
- `Database/` – EF Core context and domain models (User, Car, Brand, House, City)  
- `Tests/` – xUnit test suite covering filtering scenarios and edge cases

## Running Tests

```bash
dotnet test
```

**Key Benefits of Migration:**
- ✅ **No manual casting** required for PropertyMappings
- ✅ **Type safety** with compile-time checking
- ✅ **IntelliSense support** for better developer experience
- ✅ **Clear separation** between static mappings and dynamic data
- ✅ **100% backward compatible** - migrate gradually

## License

This project is licensed under the SuperFilter Public License v1.0, based on the MIT License. 

**Key Points:**
- ✅ **Free to use** as a dependency in your commercial or open-source projects
- ✅ **Free for personal, educational, and research purposes**
- ✅ **Attribution required**
- ❌ **Cannot create a commercial product derived/forked from the codebase of Superfilter** without explicit permission
- ❌ **Cannot sell or redistribute** as a standalone commercial offering

See the [LICENSE](LICENSE) file for full details.
