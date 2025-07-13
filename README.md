![pre-alpha](https://img.shields.io/badge/status-Available_soon_🚧-ff69b4?style=for-the-badge&logoColor=white&label=WIP)



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

### 🚀 New: Using ConfigurationBuilder (Recommended)

```csharp
using Superfilter;  // Un seul using nécessaire maintenant !
using Superfilter.Constants;

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

    // 2. Use with Superfilter (same as before)
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

### Traditional Approach

1. Create a `GlobalConfiguration` containing:
   - `PropertyMappings` — a dictionary of filter keys and their `FieldConfiguration`
   - `HasFilters` and `HasSorts` implementations holding incoming criteria
2. Instantiate `Superfilter`, call `InitializeGlobalConfiguration`, then `InitializeFieldSelectors<T>()` for each entity type
3. Call `ApplyConfiguredFilters` on an `IQueryable<T>` and optionally `ApplySorting`

## Traditional Example (Still Supported)

```csharp
using Superfilter;
using Superfilter.Entities;
using Superfilter.Constants;

// Create filter criteria with manual casting (legacy approach)
var globalConfig = new GlobalConfiguration
{
    HasFilters = new HasFiltersDto
    {
        Filters =
        [
            new FilterCriterion("id", Operator.GreaterThan, "1"),
            new FilterCriterion("carBrandName", Operator.Equals, "BMW")
        ]
    },
    HasSorts = new HasSortsDto
    {
        Sorters = [ new SortCriterion("name", "asc") ]
    },
    PropertyMappings = new Dictionary<string, FieldConfiguration>
    {
        { "id", new FieldConfiguration((Expression<Func<User, object>>)(u => u.Id), isRequired: true) },
        { "carBrandName", new FieldConfiguration((Expression<Func<User, object>>)(u => u.Car.Brand.Name)) },
        { "name", new FieldConfiguration((Expression<Func<User, object>>)(u => u.Name)) }
    }
};

// Initialize Superfilter
var superfilter = new Superfilter.Superfilter();
superfilter.InitializeGlobalConfiguration(globalConfig);
superfilter.InitializeFieldSelectors<User>();

// Apply filtering and sorting
IQueryable<User> query = context.Users;
query = superfilter.ApplyConfiguredFilters(query);
query = query.ApplySorting(globalConfig);

List<User> users = query.ToList();
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

### Static Filter Methods (For Testing/Defaults)

| Method | Operator | Description |
|--------|----------|-------------|
| `AddStaticFilterEquals(field, value)` | Equals | Static exact matching filter |
| `AddStaticFilterContains(field, value)` | Contains | Static substring search filter |
| `AddStaticFilterStartsWith(field, value)` | StartsWith | Static prefix search filter |
| `AddStaticFilterGreaterThan(field, value)` | GreaterThan | Static numeric/date comparison |
| `AddStaticFilterLessThan(field, value)` | LessThan | Static numeric/date comparison |

### Static Sort Methods (For Testing/Defaults)

| Method | Description |
|--------|-------------|
| `AddStaticSortAscending(field)` | Add static ascending sort |
| `AddStaticSortDescending(field)` | Add static descending sort |

## Legacy FieldConfiguration Constructor

For backward compatibility, the traditional `FieldConfiguration` constructor is still supported:

```csharp
// Simple property (requires manual casting)
new FieldConfiguration((Expression<Func<User, object>>)(x => x.Name))

// Required filter (requires manual casting)
new FieldConfiguration((Expression<Func<User, object>>)(x => x.Id), isRequired: true)

// Nested property (requires manual casting)
new FieldConfiguration((Expression<Func<User, object>>)(x => x.Car.Brand.Name))
```

**💡 Tip:** Use the new `ConfigurationBuilder` API instead to avoid manual casting!

## Documentation

- 📖 **[ConfigurationBuilder Examples](CONFIGURATION_BUILDER_EXAMPLES.md)** - Comprehensive examples and use cases
- 🚀 **[Integration Guide](CONFIGURATION_BUILDER_INTEGRATION.md)** - Migration guide and best practices
- 📋 **[GitHub Issue](ISSUE_PROPERTY_MAPPING_CASTING.md)** - Background on the manual casting problem

## Project Layout

- `SuperFilter/` – Core library implementation with partial class architecture
  - `Builder/` – **New:** ConfigurationBuilder API for fluent configuration
- `Database/` – EF Core context and domain models (User, Car, Brand, House, City)  
- `Tests/` – xUnit test suite covering filtering scenarios and edge cases

## Running Tests

```bash
dotnet test
```

## Migration from Manual Casting

If you're using the old approach with manual casting, migrating is simple:

```csharp
// Old approach (still works but verbose)
var config = new GlobalConfiguration
{
    PropertyMappings = new Dictionary<string, FieldConfiguration>
    {
        { "id", new FieldConfiguration((Expression<Func<User, object>>)(u => u.Id), true) },
        { "name", new FieldConfiguration((Expression<Func<User, object>>)(u => u.Name)) }
    },
    HasFilters = clientFilters,  // From client request
    HasSorts = clientSorts       // From client request
};

// New approach (clean and type-safe)
var config = SuperfilterBuilder.For<User>()
    .MapRequiredProperty("id", u => u.Id)        // No casting!
    .MapProperty("name", u => u.Name)            // Type-safe!
    .WithFilters(clientFilters)                  // Dynamic from client
    .WithSorts(clientSorts)                      // Dynamic from client
    .Build();
```

**Key Benefits of Migration:**
- ✅ **No manual casting** required for PropertyMappings
- ✅ **Type safety** with compile-time checking
- ✅ **IntelliSense support** for better developer experience
- ✅ **Clear separation** between static mappings and dynamic data
- ✅ **100% backward compatible** - migrate gradually

## License

This project does not currently have a specified license.
