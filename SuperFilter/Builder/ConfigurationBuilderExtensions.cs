using System.Linq.Expressions;
using Superfilter.Constants;
using Superfilter.Entities;

namespace Superfilter;

/// <summary>
/// Extension methods for ConfigurationBuilder to provide additional fluent API methods
/// </summary>
public static class ConfigurationBuilderExtensions
{
    /// <summary>
    /// Creates a new configuration builder for the specified entity type
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <returns>New ConfigurationBuilder instance</returns>
    public static ConfigurationBuilder<T> CreateBuilder<T>() where T : class
    {
        return new ConfigurationBuilder<T>();
    }


    /// <summary>
    /// Adds a static equals filter (for testing or default filters)
    /// </summary>
    public static ConfigurationBuilder<T> AddStaticFilterEquals<T>(
        this ConfigurationBuilder<T> builder,
        string field,
        string value) where T : class
    {
        return builder.AddStaticFilter(field, Operator.Equals, value);
    }

    /// <summary>
    /// Adds a static contains filter for string fields (for testing or default filters)
    /// </summary>
    public static ConfigurationBuilder<T> AddStaticFilterContains<T>(
        this ConfigurationBuilder<T> builder,
        string field,
        string value) where T : class
    {
        return builder.AddStaticFilter(field, Operator.Contains, value);
    }

    /// <summary>
    /// Adds a static starts with filter for string fields (for testing or default filters)
    /// </summary>
    public static ConfigurationBuilder<T> AddStaticFilterStartsWith<T>(
        this ConfigurationBuilder<T> builder,
        string field,
        string value) where T : class
    {
        return builder.AddStaticFilter(field, Operator.StartsWith, value);
    }

    /// <summary>
    /// Adds a static greater than filter for numeric/date fields (for testing or default filters)
    /// </summary>
    public static ConfigurationBuilder<T> AddStaticFilterGreaterThan<T>(
        this ConfigurationBuilder<T> builder,
        string field,
        string value) where T : class
    {
        return builder.AddStaticFilter(field, Operator.GreaterThan, value);
    }

    /// <summary>
    /// Adds a static less than filter for numeric/date fields (for testing or default filters)
    /// </summary>
    public static ConfigurationBuilder<T> AddStaticFilterLessThan<T>(
        this ConfigurationBuilder<T> builder,
        string field,
        string value) where T : class
    {
        return builder.AddStaticFilter(field, Operator.LessThan, value);
    }

    /// <summary>
    /// Adds a static ascending sort (for testing or default sorts)
    /// </summary>
    public static ConfigurationBuilder<T> AddStaticSortAscending<T>(
        this ConfigurationBuilder<T> builder,
        string field) where T : class
    {
        return builder.AddStaticSort(field, "asc");
    }

    /// <summary>
    /// Adds a static descending sort (for testing or default sorts)
    /// </summary>
    public static ConfigurationBuilder<T> AddStaticSortDescending<T>(
        this ConfigurationBuilder<T> builder,
        string field) where T : class
    {
        return builder.AddStaticSort(field, "desc");
    }

    /// <summary>
    /// Maps property and adds static filter in one call (for testing or default filters)
    /// </summary>
    public static ConfigurationBuilder<T> MapAndAddStaticFilter<T, TProperty>(
        this ConfigurationBuilder<T> builder,
        string key,
        Expression<Func<T, TProperty>> selector,
        Operator @operator,
        string value,
        bool isRequired = false) where T : class
    {
        return builder
            .MapProperty(key, selector, isRequired)
            .AddStaticFilter(key, @operator, value);
    }

    /// <summary>
    /// Maps property and adds static sort in one call (for testing or default sorts)
    /// </summary>
    public static ConfigurationBuilder<T> MapAndAddStaticSort<T, TProperty>(
        this ConfigurationBuilder<T> builder,
        string key,
        Expression<Func<T, TProperty>> selector,
        string direction = "asc",
        bool isRequired = false) where T : class
    {
        return builder
            .MapProperty(key, selector, isRequired)
            .AddStaticSort(key, direction);
    }
}