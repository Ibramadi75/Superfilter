![pre-alpha](https://img.shields.io/badge/status-Available_soon_🚧-ff69b4?style=for-the-badge&logoColor=white&label=WIP)



# Superfilter

Superfilter is a lightweight C# .NET 9.0 library for applying dynamic filtering and sorting on `IQueryable` sources. It maps textual filter criteria to strongly typed expressions, making it easy to expose flexible query capabilities in web APIs or other data-driven applications.

## Features

- Map filter keys to entity properties through `FieldConfiguration` with lambda selectors
- Supports nested navigation properties (e.g., `x => x.Car.Brand.Name`)
- Validates required filters with configurable error handling
- Built‑in operators: `Equals`, `LessThan`, `GreaterThan`, `StartsWith`, `Contains`, `IsEqualToYear`, `IsEqualToYearAndMonth`, `IsEqualToFullDate`
- Fluent API for sorting via `ApplySorting` extension method
- Works with Entity Framework and LINQ-to-Objects
- Type-safe expression building with proper type inference

## Getting Started

1. Create a `GlobalConfiguration` containing:
   - `PropertyMappings` — a dictionary of filter keys and their `FieldConfiguration`
   - `HasFilters` and `HasSorts` implementations holding incoming criteria
2. Instantiate `Superfilter`, call `InitializeGlobalConfiguration`, then `InitializeFieldSelectors<T>()` for each entity type
3. Call `ApplyConfiguredFilters` on an `IQueryable<T>` and optionally `ApplySorting`

## Example

```csharp
using Superfilter;
using Superfilter.Entities;
using Superfilter.Constants;

// Create filter criteria
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

## FieldConfiguration Constructor

The `FieldConfiguration` constructor accepts:
- `selector`: Lambda expression defining the property to filter/sort on
- `isRequired`: Optional boolean indicating if the filter is mandatory (default: false)

```csharp
// Simple property
new FieldConfiguration((Expression<Func<User, object>>)(x => x.Name))

// Required filter
new FieldConfiguration((Expression<Func<User, object>>)(x => x.Id), isRequired: true)

// Nested property
new FieldConfiguration((Expression<Func<User, object>>)(x => x.Car.Brand.Name))
```

## Project Layout

- `SuperFilter/` – Core library implementation with partial class architecture
- `Database/` – EF Core context and domain models (User, Car, Brand, House, City)  
- `Tests/` – xUnit test suite covering filtering scenarios and edge cases

## Running Tests

```bash
dotnet test
```

## License

This project does not currently have a specified license.
