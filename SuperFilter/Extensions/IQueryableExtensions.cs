using System.Linq.Expressions;
using Superfilter.Constants;
using Superfilter.Entities;

namespace Superfilter;

/// <summary>
///     IQueryable extension methods for fluent Superfilter configuration
/// </summary>
public static class IQueryableExtensions
{
    /// <summary>
    ///     Starts a fluent configuration chain for Superfilter
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="query">The IQueryable to configure</param>
    /// <returns>A QueryableWrapper for fluent configuration</returns>
    public static QueryableWrapper<T> WithSuperfilter<T>(this IQueryable<T> query) where T : class
    {
        return new QueryableWrapper<T>(query);
    }
}

/// <summary>
///     Wrapper class that provides fluent configuration for IQueryable with Superfilter
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public class QueryableWrapper<T> where T : class
{
    private readonly List<FilterCriterion> _filters = [];
    private readonly Dictionary<string, FieldConfiguration> _propertyMappings = new();
    private readonly IQueryable<T> _query;
    private readonly List<SortCriterion> _sorters = [];
    private OnErrorStrategy _onErrorStrategy = OnErrorStrategy.ThrowException;

    internal QueryableWrapper(IQueryable<T> query)
    {
        _query = query;
    }

    /// <summary>
    ///     Maps a property to a filter key with automatic type inference
    /// </summary>
    /// <typeparam name="TProperty">The property type</typeparam>
    /// <param name="key">The filter key</param>
    /// <param name="selector">Property selector expression</param>
    /// <param name="isRequired">Whether this filter is required</param>
    /// <returns>Wrapper instance for method chaining</returns>
    public QueryableWrapper<T> MapProperty<TProperty>(
        string key,
        Expression<Func<T, TProperty>> selector,
        bool isRequired = false)
    {
        Expression<Func<T, object>> objectSelector = ConvertToObjectExpression(selector);
        _propertyMappings[key] = new FieldConfiguration(objectSelector, isRequired);
        return this;
    }

    /// <summary>
    ///     Maps a property to a filter key auto-determined by selector expression
    /// </summary>
    /// <typeparam name="TProperty">The property type</typeparam>
    /// <param name="selector">Property selector expression</param>
    /// <param name="isRequired">Whether this filter is required</param>
    /// <returns>Wrapper instance for method chaining</returns>
    public QueryableWrapper<T> MapProperty<TProperty>(
        Expression<Func<T, TProperty>> selector,
        bool isRequired = false)
    {
        return MapProperty($"{Superfilter.ExtractLastWordFromDotSeparatedString(typeof(T).ToString())}.{Superfilter.ExtractPropertyPathFromSelectorString(selector.ToString())}", selector, isRequired);
    }

    /// <summary>
    ///     Maps a property as required filter
    /// </summary>
    /// <typeparam name="TProperty">The property type</typeparam>
    /// <param name="key">The filter key</param>
    /// <param name="selector">Property selector expression</param>
    /// <returns>Wrapper instance for method chaining</returns>
    public QueryableWrapper<T> MapRequiredProperty<TProperty>(
        string key,
        Expression<Func<T, TProperty>> selector)
    {
        return MapProperty(key, selector, true);
    }

    /// <summary>
    ///     Maps a property as required filter with auto-determined key
    /// </summary>
    /// <typeparam name="TProperty">The property type</typeparam>
    /// <param name="selector">Property selector expression</param>
    /// <returns>Wrapper instance for method chaining</returns>
    public QueryableWrapper<T> MapRequiredProperty<TProperty>(
        Expression<Func<T, TProperty>> selector)
    {
        return MapProperty(selector, true);
    }

    /// <summary>
    ///     Sets the HasFilters implementation with dynamic filter criteria from client
    ///     This method applies filters immediately and returns the filtered IQueryable
    /// </summary>
    /// <param name="hasFilters">Filter criteria from client request</param>
    /// <returns>The filtered IQueryable</returns>
    public IQueryable<T> WithFilters(IHasFilters hasFilters)
    {
        _filters.Clear();
        _filters.AddRange(hasFilters.Filters);
        return ApplyConfiguration();
    }

    /// <summary>
    ///     Sets the HasSorts implementation with dynamic sort criteria from client
    /// </summary>
    /// <param name="hasSorts">Sort criteria from client request</param>
    /// <returns>Wrapper instance for method chaining</returns>
    public QueryableWrapper<T> WithSorts(IHasSorts hasSorts)
    {
        _sorters.Clear();
        _sorters.AddRange(hasSorts.Sorters);
        return this;
    }

    /// <summary>
    ///     Adds a static filter criterion
    /// </summary>
    /// <param name="field">Field name</param>
    /// <param name="operator">Filter operator</param>
    /// <param name="value">Filter value</param>
    /// <returns>Wrapper instance for method chaining</returns>
    public QueryableWrapper<T> AddStaticFilter(string field, Operator @operator, string value)
    {
        _filters.Add(new FilterCriterion(field, @operator, value));
        return this;
    }

    /// <summary>
    ///     Adds a static sort criterion
    /// </summary>
    /// <param name="field">Field name</param>
    /// <param name="direction">Sort direction (asc/desc)</param>
    /// <returns>Wrapper instance for method chaining</returns>
    public QueryableWrapper<T> AddStaticSort(string field, string direction = "asc")
    {
        _sorters.Add(new SortCriterion(field, direction));
        return this;
    }

    /// <summary>
    ///     Sets the error handling strategy
    /// </summary>
    /// <param name="strategy">Error handling strategy</param>
    /// <returns>Wrapper instance for method chaining</returns>
    public QueryableWrapper<T> WithErrorStrategy(OnErrorStrategy strategy)
    {
        _onErrorStrategy = strategy;
        return this;
    }

    /// <summary>
    ///     Applies the configuration and returns the filtered/sorted IQueryable
    /// </summary>
    /// <returns>The configured IQueryable</returns>
    public IQueryable<T> Build()
    {
        return ApplyConfiguration();
    }

    /// <summary>
    ///     Applies all configured filters and sorts to the query
    /// </summary>
    /// <returns>The filtered and sorted IQueryable</returns>
    private IQueryable<T> ApplyConfiguration()
    {
        GlobalConfiguration config = new()
        {
            PropertyMappings = _propertyMappings,
            MissingOnStrategy = _onErrorStrategy,
            HasFilters = new FilterContainer(_filters),
            HasSorts = new SortContainer(_sorters)
        };

        Superfilter superfilter = new();
        superfilter.InitializeGlobalConfiguration(config);
        superfilter.InitializeFieldSelectors<T>();

        IQueryable<T> result = _query;

        // Apply filters if any
        if (_filters.Count > 0) result = superfilter.ApplyConfiguredFilters(result);

        // Apply sorts if any
        if (_sorters.Count > 0) result = result.ApplySorting(superfilter);

        return result;
    }

    /// <summary>
    ///     Converts a typed expression to Expression&lt;Func&lt;T, object&gt;&gt;
    /// </summary>
    private static Expression<Func<T, object>> ConvertToObjectExpression<TProperty>(
        Expression<Func<T, TProperty>> expression)
    {
        ParameterExpression parameter = expression.Parameters[0];
        Expression body = expression.Body;

        // If the property is not object type, wrap it in a conversion
        if (body.Type != typeof(object)) body = Expression.Convert(body, typeof(object));

        return Expression.Lambda<Func<T, object>>(body, parameter);
    }

    /// <summary>
    ///     Internal implementation of IHasFilters for the wrapper
    /// </summary>
    private class FilterContainer(List<FilterCriterion> filters) : IHasFilters
    {
        public List<FilterCriterion> Filters { get; set; } = filters;
    }

    /// <summary>
    ///     Internal implementation of IHasSorts for the wrapper
    /// </summary>
    private class SortContainer(List<SortCriterion> sorters) : IHasSorts
    {
        public List<SortCriterion> Sorters { get; set; } = sorters;
    }
}