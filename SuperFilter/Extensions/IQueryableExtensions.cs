using System.Collections;
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

    /// <summary>
    ///     Starts a fluent configuration chain for Superfilter with a request object.
    ///     The request can implement IHasFilters, IHasSorts, and/or IHasPagination.
    ///     This allows calling WithFilters(), WithSorts(), and ApplyPagination() without parameters.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <typeparam name="TRequest">The request type</typeparam>
    /// <param name="query">The IQueryable to configure</param>
    /// <param name="request">The request object containing filters, sorts, and/or pagination</param>
    /// <returns>A QueryableWrapper for fluent configuration</returns>
    public static QueryableWrapper<T> WithSuperfilter<T, TRequest>(this IQueryable<T> query, TRequest request)
        where T : class
        where TRequest : class
    {
        return new QueryableWrapper<T>(query, request);
    }

    /// <summary>
    ///     Applies pagination using IHasPagination interface (page-based)
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="query">The IQueryable to paginate</param>
    /// <param name="hasPagination">Pagination criteria</param>
    /// <returns>The paginated IQueryable</returns>
    public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> query, IHasPagination hasPagination)
    {
        return query
            .Skip(hasPagination.Pagination.Skip)
            .Take(hasPagination.Pagination.Take);
    }

    /// <summary>
    ///     Applies pagination using page number and page size (page-based)
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="query">The IQueryable to paginate</param>
    /// <param name="pageNumber">The page number (1-based)</param>
    /// <param name="pageSize">The number of items per page</param>
    /// <returns>The paginated IQueryable</returns>
    public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> query, int pageNumber, int pageSize)
    {
        Pagination pagination = new(pageNumber, pageSize);
        return query
            .Skip(pagination.Skip)
            .Take(pagination.Take);
    }

    /// <summary>
    ///     Applies offset-based pagination using skip and take values
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="query">The IQueryable to paginate</param>
    /// <param name="skip">Number of items to skip</param>
    /// <param name="take">Number of items to take</param>
    /// <returns>The paginated IQueryable</returns>
    public static IQueryable<T> ApplyOffsetPagination<T>(this IQueryable<T> query, int skip, int take)
    {
        return query
            .Skip(skip)
            .Take(take);
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

    // Stored request interfaces for parameterless method calls
    private readonly IHasFilters? _storedHasFilters;
    private readonly IHasSorts? _storedHasSorts;
    private readonly IHasPagination? _storedHasPagination;

    internal QueryableWrapper(IQueryable<T> query)
    {
        _query = query;
    }

    internal QueryableWrapper(IQueryable<T> query, object request)
    {
        _query = query;
        _storedHasFilters = request as IHasFilters;
        _storedHasSorts = request as IHasSorts;
        _storedHasPagination = request as IHasPagination;
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
    ///     Applies filters from the stored request object (passed to WithSuperfilter).
    ///     Returns the wrapper for method chaining.
    /// </summary>
    /// <returns>Wrapper instance for method chaining</returns>
    /// <exception cref="InvalidOperationException">Thrown when no IHasFilters was provided to WithSuperfilter</exception>
    public QueryableWrapper<T> WithFilters()
    {
        if (_storedHasFilters == null)
            throw new InvalidOperationException(
                "No IHasFilters was provided. Either pass a request implementing IHasFilters to WithSuperfilter(), or use WithFilters(IHasFilters) overload.");

        _filters.Clear();
        _filters.AddRange(_storedHasFilters.Filters);
        return this;
    }

    /// <summary>
    ///     Sets the HasFilters implementation with dynamic filter criteria from client.
    ///     Returns the wrapper for method chaining.
    /// </summary>
    /// <param name="hasFilters">Filter criteria from client request</param>
    /// <returns>Wrapper instance for method chaining</returns>
    public QueryableWrapper<T> WithFilters(IHasFilters hasFilters)
    {
        _filters.Clear();
        _filters.AddRange(hasFilters.Filters);
        return this;
    }

    /// <summary>
    ///     Applies sorts from the stored request object (passed to WithSuperfilter).
    ///     Returns the wrapper for method chaining.
    /// </summary>
    /// <returns>Wrapper instance for method chaining</returns>
    /// <exception cref="InvalidOperationException">Thrown when no IHasSorts was provided to WithSuperfilter</exception>
    public QueryableWrapper<T> WithSorts()
    {
        if (_storedHasSorts == null)
            throw new InvalidOperationException(
                "No IHasSorts was provided. Either pass a request implementing IHasSorts to WithSuperfilter(), or use WithSorts(IHasSorts) overload.");

        _sorters.Clear();
        _sorters.AddRange(_storedHasSorts.Sorters);
        return this;
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
    ///     Shortcut: Applies filters, sorts, and pagination in one call.
    ///     Equivalent to ApplyFilters().ApplyPagination().
    /// </summary>
    /// <returns>The filtered, sorted, and paginated IQueryable</returns>
    /// <exception cref="InvalidOperationException">Thrown when no IHasPagination was provided to WithSuperfilter</exception>
    public IQueryable<T> ApplyFiltersAndPagination()
    {
        return ApplyFilters().ApplyPagination();
    }

    /// <summary>
    ///     Shortcut: Applies filters, sorts, and pagination in one call.
    /// </summary>
    /// <param name="hasPagination">Pagination criteria</param>
    /// <returns>The filtered, sorted, and paginated IQueryable</returns>
    public IQueryable<T> ApplyFiltersAndPagination(IHasPagination hasPagination)
    {
        return ApplyFilters().ApplyPagination(hasPagination);
    }

    /// <summary>
    ///     Shortcut: Applies filters, sorts, and pagination in one call.
    /// </summary>
    /// <param name="pageNumber">The page number (1-based)</param>
    /// <param name="pageSize">The number of items per page</param>
    /// <returns>The filtered, sorted, and paginated IQueryable</returns>
    public IQueryable<T> ApplyFiltersAndPagination(int pageNumber, int pageSize)
    {
        return ApplyFilters().ApplyPagination(pageNumber, pageSize);
    }

    /// <summary>
    ///     Shortcut: Applies filters, sorts, and offset-based pagination in one call.
    /// </summary>
    /// <param name="skip">Number of items to skip</param>
    /// <param name="take">Number of items to take</param>
    /// <returns>The filtered, sorted, and paginated IQueryable</returns>
    public IQueryable<T> ApplyFiltersAndOffsetPagination(int skip, int take)
    {
        return ApplyFilters().ApplyOffsetPagination(skip, take);
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
    ///     Applies filters and sorts, returns a FilteredQueryable that can be used directly
    ///     or chained with ApplyPagination().
    ///     Automatically uses stored filters/sorts from the request if available.
    /// </summary>
    /// <returns>A FilteredQueryable for optional pagination chaining</returns>
    public FilteredQueryable<T> ApplyFilters()
    {
        return new FilteredQueryable<T>(ApplyConfiguration(), _storedHasPagination);
    }

    /// <summary>
    ///     Applies all configured filters and sorts to the query.
    ///     Automatically uses stored filters/sorts from the request if available.
    /// </summary>
    /// <returns>The filtered and sorted IQueryable</returns>
    private IQueryable<T> ApplyConfiguration()
    {
        // Use stored filters if no explicit filters were added
        List<FilterCriterion> filtersToApply = _filters.Count > 0
            ? _filters
            : _storedHasFilters?.Filters ?? [];

        // Use stored sorts if no explicit sorts were added
        List<SortCriterion> sortsToApply = _sorters.Count > 0
            ? _sorters
            : _storedHasSorts?.Sorters ?? [];

        GlobalConfiguration config = new()
        {
            PropertyMappings = _propertyMappings,
            MissingOnStrategy = _onErrorStrategy,
            HasFilters = new FilterContainer(filtersToApply),
            HasSorts = new SortContainer(sortsToApply)
        };

        Superfilter superfilter = new();
        superfilter.InitializeGlobalConfiguration(config);
        superfilter.InitializeFieldSelectors<T>();

        IQueryable<T> result = _query;

        // Apply filters if any
        if (filtersToApply.Count > 0) result = superfilter.ApplyConfiguredFilters(result);

        // Apply sorts if any
        if (sortsToApply.Count > 0) result = result.ApplySorting(superfilter);

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
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
    }

    /// <summary>
    ///     Internal implementation of IHasSorts for the wrapper
    /// </summary>
    private class SortContainer(List<SortCriterion> sorters) : IHasSorts
    {
        public List<SortCriterion> Sorters { get; set; } = sorters;
    }
}

/// <summary>
///     Represents a filtered IQueryable that can optionally have pagination applied.
///     Can be used directly as IQueryable or chained with ApplyPagination().
///     Properly forwards async enumeration for EF Core compatibility.
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public class FilteredQueryable<T> : IQueryable<T>, IAsyncEnumerable<T>
{
    private readonly IQueryable<T> _query;
    private readonly IHasPagination? _storedPagination;

    internal FilteredQueryable(IQueryable<T> query, IHasPagination? pagination)
    {
        _query = query;
        _storedPagination = pagination;
    }

    #region IQueryable Implementation

    public Type ElementType => _query.ElementType;
    public Expression Expression => _query.Expression;
    public IQueryProvider Provider => _query.Provider;

    public IEnumerator<T> GetEnumerator() => _query.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion

    #region IAsyncEnumerable Implementation

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        if (_query is IAsyncEnumerable<T> asyncEnumerable)
            return asyncEnumerable.GetAsyncEnumerator(cancellationToken);

        // Fallback for non-async sources (e.g., in-memory lists in tests)
        return new SyncToAsyncEnumerator<T>(_query.GetEnumerator());
    }

    private class SyncToAsyncEnumerator<TItem>(IEnumerator<TItem> inner) : IAsyncEnumerator<TItem>
    {
        public TItem Current => inner.Current;
        public ValueTask<bool> MoveNextAsync() => new(inner.MoveNext());
        public ValueTask DisposeAsync()
        {
            inner.Dispose();
            return ValueTask.CompletedTask;
        }
    }

    #endregion

    #region Pagination Methods

    /// <summary>
    ///     Applies pagination from the stored request object.
    /// </summary>
    /// <returns>The paginated IQueryable</returns>
    /// <exception cref="InvalidOperationException">Thrown when no pagination was provided</exception>
    public IQueryable<T> ApplyPagination()
    {
        if (_storedPagination == null)
            throw new InvalidOperationException(
                "No IHasPagination was provided. Either pass a request implementing IHasPagination to WithSuperfilter(), or use ApplyPagination(IHasPagination) overload.");

        return _query
            .Skip(_storedPagination.Pagination.Skip)
            .Take(_storedPagination.Pagination.Take);
    }

    /// <summary>
    ///     Applies pagination using IHasPagination interface.
    /// </summary>
    /// <param name="hasPagination">Pagination criteria</param>
    /// <returns>The paginated IQueryable</returns>
    public IQueryable<T> ApplyPagination(IHasPagination hasPagination)
    {
        return _query
            .Skip(hasPagination.Pagination.Skip)
            .Take(hasPagination.Pagination.Take);
    }

    /// <summary>
    ///     Applies pagination using page number and page size.
    /// </summary>
    /// <param name="pageNumber">The page number (1-based)</param>
    /// <param name="pageSize">The number of items per page</param>
    /// <returns>The paginated IQueryable</returns>
    public IQueryable<T> ApplyPagination(int pageNumber, int pageSize)
    {
        Pagination pagination = new(pageNumber, pageSize);
        return _query
            .Skip(pagination.Skip)
            .Take(pagination.Take);
    }

    /// <summary>
    ///     Applies offset-based pagination.
    /// </summary>
    /// <param name="skip">Number of items to skip</param>
    /// <param name="take">Number of items to take</param>
    /// <returns>The paginated IQueryable</returns>
    public IQueryable<T> ApplyOffsetPagination(int skip, int take)
    {
        return _query
            .Skip(skip)
            .Take(take);
    }

    #endregion
}