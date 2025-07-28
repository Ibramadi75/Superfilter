using System.Linq.Expressions;
using System.Reflection;
using Superfilter.Constants;
using Superfilter.Entities;
using Superfilter.ExpressionBuilders;

namespace Superfilter;

internal static class SuperfilterExtensions
{
    public static IQueryable<T> FilterProperty<T, TProperty>(
        this IQueryable<T> query,
        GlobalConfiguration globalConfiguration,
        Expression<Func<T, TProperty>> propertyExpression,
        bool isRequired)
    {
        Expression body = propertyExpression.Body is UnaryExpression unary ? unary.Operand : propertyExpression.Body;
        string propertyName = ((MemberExpression)body).Member.Name;

        (string? propertyMapKey, FieldConfiguration? propertyMap) = globalConfiguration.PropertyMappings
            .FirstOrDefault(x => x.Value.Selector.ToString() == propertyExpression.ToString());

        if (propertyMapKey == null || propertyMap == null)
            return query;

        FilterCriterion? filter = globalConfiguration.HasFilters.Filters
            .FirstOrDefault(filters => string.Equals(filters.Field, propertyMapKey, StringComparison.CurrentCultureIgnoreCase));

        if (filter == null || (string.IsNullOrEmpty(filter.Value) && filter.Operator != Operator.IsNull && filter.Operator != Operator.IsNotNull && filter.Operator != Operator.IsEmpty && filter.Operator != Operator.IsNotEmpty))
        {
            if (propertyMap.IsRequired)
                throw new SuperfilterException($"Filter {propertyName} is required.");
            return query;
        }

        if (body is not MemberExpression memberExpression)
            throw new InvalidOperationException($"Invalid expression for sorting: {body}");

        ParameterExpression parameter = Expression.Parameter(typeof(T), typeof(T).ToString());
        Expression propertyAccess = GetNestedPropertyExpression(parameter, RemoveUntilFirstDot(memberExpression.ToString()));
        Expression filterExpression = Builder.GetExpression<TProperty>((MemberExpression)propertyAccess, filter.Value, filter.Operator);

        Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(filterExpression, parameter);

        query = query.Where(lambda);

        return query;
    }

    public static IQueryable<T> ApplySorting<T>(
        this IQueryable<T> query,
        Superfilter superfilter)
    {
        if (superfilter.InternalGlobalConfiguration == null)
            throw new SuperfilterException("Superfilter is not properly initialized");

        return query.ApplySorting(superfilter.InternalGlobalConfiguration);
    }

    public static IQueryable<T> ApplySorting<T>(
        this IQueryable<T> query,
        GlobalConfiguration globalConfiguration)
    {
        if (globalConfiguration.HasSorts.Sorters.Count == 0)
            throw new SuperfilterException("At least 1 sorter must be specified");

        foreach (SortCriterion sorter in globalConfiguration.HasSorts.Sorters)
        {
            if (!globalConfiguration.PropertyMappings.TryGetValue(sorter.Field, out FieldConfiguration? property)) continue;

            Expression body = property.Selector.Body is UnaryExpression unary ? unary.Operand : property.Selector.Body;

            if (body is not MemberExpression memberExpression)
                throw new InvalidOperationException($"Invalid expression for sorting: {property.Selector.Body}");

            ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
            Type propertyType = ((PropertyInfo)memberExpression.Member).PropertyType;

            Expression propertyAccess = GetNestedPropertyExpression(parameter, RemoveUntilFirstDot(memberExpression.ToString()));
            LambdaExpression lambda = Expression.Lambda(Expression.Convert(propertyAccess, propertyType), parameter);

            string methodName = query.Expression.Type == typeof(IOrderedQueryable<T>)
                ? sorter.Dir.Equals("asc", StringComparison.CurrentCultureIgnoreCase) ? "ThenBy" : "ThenByDescending"
                : sorter.Dir.Equals("asc", StringComparison.CurrentCultureIgnoreCase)
                    ? "OrderBy"
                    : "OrderByDescending";

            MethodInfo method = typeof(Queryable)
                .GetMethods()
                .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), propertyType);

            query = (IQueryable<T>)method.Invoke(null, [query, lambda])!;
        }

        return query;
    }

    private static Expression GetNestedPropertyExpression(Expression parameter, string propertyPath)
    {
        return propertyPath.Split('.').Aggregate(parameter, Expression.Property);
    }

    private static string RemoveUntilFirstDot(string input)
    {
        return !input.Contains('.') ? input : input[(input.IndexOf('.') + 1)..];
    }
}