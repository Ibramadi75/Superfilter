<svg width="500" height="60" xmlns="http://www.w3.org/2000/svg">
  <style>
    @keyframes blink {
      0%, 100% { opacity: 1; }
      50% { opacity: 0.3; }
    }
    .text {
      font-family: sans-serif;
      font-size: 20px;
      animation: blink 1.5s infinite;
      fill: #ff4081;
    }
    .background {
      fill: #1e1e1e;
    }
  </style>
  <rect class="background" width="100%" height="100%" rx="10" />
  <text x="50%" y="50%" text-anchor="middle" alignment-baseline="central" class="text">
    🚧 Work in progress – Soon available
  </text>
</svg>

# SuperFilter

SuperFilter is a lightweight C# library for applying dynamic filtering and sorting on `IQueryable` sources. It maps textual filter criteria to strongly typed expressions, making it easy to expose flexible query capabilities in web APIs or other data-driven applications.

## Features

- Map filter keys to entity properties through `FieldConfiguration`.
- Supports nested navigation properties (e.g., `x => x.Car.Brand.Name`).
- Validates required filters.
- Built‑in operators: `Equals`, `LessThan`, `GreaterThan`, `StartsWith`, `Contains`, `IsEqualToYear`, `IsEqualToYearAndMonth`, `IsEqualToFullDate`.
- Optional sorting via `ApplySorting`.
- Works with Entity Framework and LINQ-to-Objects.

## Getting Started

1. Create a `GlobalConfiguration` containing:
   - `PropertyMappings` — a dictionary of filter keys and their `FieldConfiguration`.
   - `HasFilters` and `HasSorts` implementations holding incoming criteria.
2. Instantiate `SuperFilter`, call `SetGlobalConfiguration`, then `SetupFieldConfiguration<T>()` for each entity type.
3. Call `ApplyFilters` on an `IQueryable<T>` and optionally `ApplySorting`.

## Example

```csharp
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
        { "id", new FieldConfiguration { EntityPropertyName = nameof(User.Id), Selector = (Expression<Func<User, object>>)(u => u.Id) } },
        { "carBrandName", new FieldConfiguration { EntityPropertyName = nameof(User.Car.Brand.Name), Selector = (Expression<Func<User, object>>)(u => u.Car.Brand.Name) } },
        { "name", new FieldConfiguration { EntityPropertyName = nameof(User.Name), Selector = (Expression<Func<User, object>>)(u => u.Name) } }
    }
};

var superFilter = new SuperFilter();
superFilter.SetGlobalConfiguration(globalConfig);
superFilter.SetupFieldConfiguration<User>();

IQueryable<User> query = context.Users;
query = superFilter.ApplyFilters(query);
query = query.ApplySorting(globalConfig);

List<User> users = query.ToList();
```

## Project Layout

- `Tools/` – implementation of the SuperFilter library.
- `Database/` – sample EF Core models used in tests.
- `WebApplication/` – minimal API showing integration.
- `Tests/` – unit tests demonstrating filtering and sorting features.

## Running Tests

```bash
dotnet test
```

## License

This project does not currently have a specified license.
