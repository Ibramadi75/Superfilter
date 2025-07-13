using Database.Models;
using Superfilter; // Seul using nécessaire maintenant !
using Superfilter.Constants;
using Superfilter.Entities;

namespace Examples;

/// <summary>
/// Exemple d'utilisation de Superfilter avec seulement "using Superfilter;"
/// </summary>
public class SimpleUsageExample
{
    public static void ShowSimpleUsage()
    {
        // Simulation de données venant d'un endpoint client
        var clientFilters = new HasFiltersDto
        {
            Filters = 
            [
                new FilterCriterion("name", Operator.Contains, "Alice"),
                new FilterCriterion("age", Operator.GreaterThan, "25")
            ]
        };

        // Configuration avec SuperfilterBuilder - AUCUN CASTING MANUEL !
        var config = SuperfilterBuilder.For<User>()
            .MapProperty("name", x => x.Name)
            .MapProperty("age", x => x.MoneyAmount) // Utilisation de MoneyAmount comme "age"
            .WithFilters(clientFilters)
            .Build();

        // Utilisation normale
        var superfilter = new Superfilter.Superfilter();
        superfilter.InitializeGlobalConfiguration(config);
        superfilter.InitializeFieldSelectors<User>();

        // Simulation d'une requête (normalement depuis EF Core DbContext)
        var users = GetSampleUsers().AsQueryable();
        var filteredUsers = superfilter.ApplyConfiguredFilters(users);

        Console.WriteLine($"Utilisateurs filtrés: {filteredUsers.Count()}");
    }

    private static List<User> GetSampleUsers()
    {
        return new List<User>
        {
            new() { Id = 1, Name = "Alice", MoneyAmount = 30 },
            new() { Id = 2, Name = "Bob", MoneyAmount = 20 },
            new() { Id = 3, Name = "Alice Junior", MoneyAmount = 35 }
        };
    }
}