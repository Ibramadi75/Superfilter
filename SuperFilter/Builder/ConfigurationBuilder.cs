using System.Linq.Expressions;
using Superfilter.Constants;
using Superfilter.Entities;

namespace Superfilter;

/// <summary>
/// Fluent builder for creating GlobalConfiguration without manual casting
/// </summary>
/// <typeparam name="T">The entity type being configured</typeparam>
public class ConfigurationBuilder<T> where T : class
{
    private readonly Dictionary<string, FieldConfiguration> _propertyMappings = new();
    private readonly List<FilterCriterion> _filters = new();
    private readonly List<SortCriterion> _sorters = new();
    private OnErrorStrategy _onErrorStrategy = OnErrorStrategy.ThrowException;

    /// <summary>
    /// Maps a property to a filter key with automatic type inference
    /// </summary>
    /// <typeparam name="TProperty">The property type</typeparam>
    /// <param name="key">The filter key</param>
    /// <param name="selector">Property selector expression</param>
    /// <param name="isRequired">Whether this filter is required</param>
    /// <returns>Builder instance for method chaining</returns>
    public ConfigurationBuilder<T> MapProperty<TProperty>(
        string key, 
        Expression<Func<T, TProperty>> selector, 
        bool isRequired = false)
    {
        // Convert to Expression<Func<T, object>> to match FieldConfiguration
        var objectSelector = ConvertToObjectExpression(selector);
        _propertyMappings[key] = new FieldConfiguration(objectSelector, isRequired);
        return this;
    }

    /// <summary>
    /// Maps a property as required filter
    /// </summary>
    /// <typeparam name="TProperty">The property type</typeparam>
    /// <param name="key">The filter key</param>
    /// <param name="selector">Property selector expression</param>
    /// <returns>Builder instance for method chaining</returns>
    public ConfigurationBuilder<T> MapRequiredProperty<TProperty>(
        string key, 
        Expression<Func<T, TProperty>> selector)
    {
        return MapProperty(key, selector, isRequired: true);
    }

    /// <summary>
    /// Sets the HasFilters implementation with dynamic filter criteria from client
    /// </summary>
    /// <param name="hasFilters">Filter criteria from client request</param>
    /// <returns>Builder instance for method chaining</returns>
    public ConfigurationBuilder<T> WithFilters(IHasFilters hasFilters)
    {
        _filters.Clear();
        if (hasFilters?.Filters != null)
        {
            _filters.AddRange(hasFilters.Filters);
        }
        return this;
    }

    /// <summary>
    /// Sets the HasSorts implementation with dynamic sort criteria from client
    /// </summary>
    /// <param name="hasSorts">Sort criteria from client request</param>
    /// <returns>Builder instance for method chaining</returns>
    public ConfigurationBuilder<T> WithSorts(IHasSorts hasSorts)
    {
        _sorters.Clear();
        if (hasSorts?.Sorters != null)
        {
            _sorters.AddRange(hasSorts.Sorters);
        }
        return this;
    }

    /// <summary>
    /// Adds a static filter criterion (for testing or default filters)
    /// </summary>
    /// <param name="field">Field name</param>
    /// <param name="operator">Filter operator</param>
    /// <param name="value">Filter value</param>
    /// <returns>Builder instance for method chaining</returns>
    public ConfigurationBuilder<T> AddStaticFilter(string field, Operator @operator, string value)
    {
        _filters.Add(new FilterCriterion(field, @operator, value));
        return this;
    }

    /// <summary>
    /// Adds a static sort criterion (for testing or default sorts)
    /// </summary>
    /// <param name="field">Field name</param>
    /// <param name="direction">Sort direction (asc/desc)</param>
    /// <returns>Builder instance for method chaining</returns>
    public ConfigurationBuilder<T> AddStaticSort(string field, string direction = "asc")
    {
        _sorters.Add(new SortCriterion(field, direction));
        return this;
    }

    /// <summary>
    /// Sets the error handling strategy
    /// </summary>
    /// <param name="strategy">Error handling strategy</param>
    /// <returns>Builder instance for method chaining</returns>
    public ConfigurationBuilder<T> WithErrorStrategy(OnErrorStrategy strategy)
    {
        _onErrorStrategy = strategy;
        return this;
    }

    /// <summary>
    /// Builds the final GlobalConfiguration
    /// </summary>
    /// <returns>Configured GlobalConfiguration instance</returns>
    public GlobalConfiguration Build()
    {
        return new GlobalConfiguration
        {
            PropertyMappings = _propertyMappings,
            MissingOnStrategy = _onErrorStrategy,
            HasFilters = new FilterContainer(_filters),
            HasSorts = new SortContainer(_sorters)
        };
    }

    /// <summary>
    /// Builds a GlobalConfiguration with only PropertyMappings (for reusable configurations)
    /// </summary>
    /// <returns>GlobalConfiguration with empty filters/sorts that can be populated later</returns>
    public GlobalConfiguration BuildMappingsOnly()
    {
        return new GlobalConfiguration
        {
            PropertyMappings = _propertyMappings,
            MissingOnStrategy = _onErrorStrategy,
            HasFilters = new FilterContainer(new List<FilterCriterion>()),
            HasSorts = new SortContainer(new List<SortCriterion>())
        };
    }

    /// <summary>
    /// Converts a typed expression to Expression&lt;Func&lt;T, object&gt;&gt;
    /// </summary>
    private static Expression<Func<T, object>> ConvertToObjectExpression<TProperty>(
        Expression<Func<T, TProperty>> expression)
    {
        var parameter = expression.Parameters[0];
        Expression body = expression.Body;

        // If the property is not object type, wrap it in a conversion
        if (body.Type != typeof(object))
        {
            body = Expression.Convert(body, typeof(object));
        }

        return Expression.Lambda<Func<T, object>>(body, parameter);
    }

    /// <summary>
    /// Internal implementation of IHasFilters for the builder
    /// </summary>
    private class FilterContainer : IHasFilters
    {
        public List<FilterCriterion> Filters { get; set; }

        public FilterContainer(List<FilterCriterion> filters)
        {
            Filters = filters;
        }
    }

    /// <summary>
    /// Internal implementation of IHasSorts for the builder
    /// </summary>
    private class SortContainer : IHasSorts
    {
        public List<SortCriterion> Sorters { get; set; }

        public SortContainer(List<SortCriterion> sorters)
        {
            Sorters = sorters;
        }
    }
}