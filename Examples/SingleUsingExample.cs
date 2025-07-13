// ========================================
// EXEMPLE D'UTILISATION SIMPLIFIÉE
// Un seul using Superfilter; nécessaire !
// ========================================

using Superfilter;
using Superfilter.Constants;
using Superfilter.Entities;

namespace Examples;

public class Person
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public DateTime BirthDate { get; set; }
    public Car? Car { get; set; }
}

public class Car
{
    public string Brand { get; set; } = string.Empty;
    public int Year { get; set; }
}

/// <summary>
/// Démonstration de l'API simplifiée avec un seul using
/// </summary>
public class SingleUsingExample
{
    public static void DemonstrateSimpleAPI()
    {
        Console.WriteLine("=== Superfilter avec un seul using Superfilter; ===");

        // 1. Données de test
        var people = new List<Person>
        {
            new() { Id = 1, Name = "Alice", Age = 25, BirthDate = new DateTime(1999, 1, 15), Car = new Car { Brand = "Ford", Year = 2020 } },
            new() { Id = 2, Name = "Bob", Age = 30, BirthDate = new DateTime(1994, 6, 20), Car = new Car { Brand = "BMW", Year = 2018 } },
            new() { Id = 3, Name = "Charlie", Age = 35, BirthDate = new DateTime(1989, 11, 10), Car = new Car { Brand = "Ford", Year = 2021 } },
            new() { Id = 4, Name = "Diana", Age = 28, BirthDate = new DateTime(1996, 3, 8), Car = new Car { Brand = "Toyota", Year = 2019 } }
        }.AsQueryable();

        // 2. Critères de filtrage venant du client (API endpoint)
        var clientFilters = new HasFiltersDto
        {
            Filters = new List<FilterCriterion>
            {
                new("age", Operator.GreaterThan, "25"),
                new("carBrand", Operator.Equals, "Ford")
            }
        };

        // 3. Configuration SANS CASTING MANUEL grâce au Builder !
        var config = SuperfilterBuilder.For<Person>()
            .MapProperty("name", p => p.Name)           // Type inference automatique
            .MapProperty("age", p => p.Age)             // Gère les int sans casting
            .MapProperty("birthDate", p => p.BirthDate) // Gère les DateTime sans casting
            .MapProperty("carBrand", p => p.Car!.Brand) // Navigation properties OK
            .WithFilters(clientFilters)                 // Données dynamiques du client
            .Build();

        // 4. Utilisation standard de Superfilter
        var superfilter = new Superfilter.Superfilter();
        superfilter.InitializeGlobalConfiguration(config);
        superfilter.InitializeFieldSelectors<Person>();

        // 5. Application des filtres
        var filteredPeople = superfilter.ApplyConfiguredFilters(people);
        var results = filteredPeople.ToList();

        // 6. Affichage des résultats
        Console.WriteLine($"Nombre de personnes filtrées: {results.Count}");
        foreach (var person in results)
        {
            Console.WriteLine($"- {person.Name}, {person.Age} ans, {person.Car?.Brand}");
        }

        Console.WriteLine("\\n=== Avantages de la nouvelle API ===");
        Console.WriteLine("✅ Un seul using Superfilter;");
        Console.WriteLine("✅ Aucun casting manuel (Expression<Func<User, object>>)");
        Console.WriteLine("✅ IntelliSense complet sur les propriétés");
        Console.WriteLine("✅ Type inference automatique");
        Console.WriteLine("✅ Support des navigation properties");
        Console.WriteLine("✅ API fluide et lisible");
    }
}

// ========================================
// COMPARAISON ANCIENNE vs NOUVELLE API
// ========================================

public class ComparisonExample
{
    public static void ShowComparison()
    {
        Console.WriteLine("=== COMPARAISON ANCIENNE vs NOUVELLE API ===\\n");

        Console.WriteLine("❌ ANCIENNE MÉTHODE (verbosité et casting manuel):");
        Console.WriteLine(@"
using System.Linq.Expressions;
using Superfilter;
using Superfilter.Entities;

var globalConfiguration = new GlobalConfiguration()
{
    HasFilters = new HasFiltersDto
    {
        Filters = [new FilterCriterion(""age"", Operator.GreaterThan, ""25"")]
    }
};

var propertyMappings = new Dictionary<string, FieldConfiguration>()
{
    { ""name"", new FieldConfiguration((Expression<Func<Person, object>>)(x => x.Name)) },
    { ""age"", new FieldConfiguration((Expression<Func<Person, object>>)(x => x.Age)) },
    { ""carBrand"", new FieldConfiguration((Expression<Func<Person, object>>)(x => x.Car!.Brand)) }
};
globalConfiguration.PropertyMappings = propertyMappings;");

        Console.WriteLine("\\n✅ NOUVELLE MÉTHODE (simple et propre):");
        Console.WriteLine(@"
using Superfilter;  // Un seul using !

var filters = new HasFiltersDto
{
    Filters = [new FilterCriterion(""age"", Operator.GreaterThan, ""25"")]
};

var config = SuperfilterBuilder.For<Person>()
    .MapProperty(""name"", x => x.Name)           // Aucun casting !
    .MapProperty(""age"", x => x.Age)             // Type inference !
    .MapProperty(""carBrand"", x => x.Car!.Brand) // Navigation OK !
    .WithFilters(filters)                         // Fluent API !
    .Build();");

        Console.WriteLine("\\n🎉 RÉSULTAT : Code 3x plus court et plus lisible !");
    }
}