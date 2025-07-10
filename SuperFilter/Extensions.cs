using System.Linq.Expressions;
using System.Reflection;

namespace SuperFilter;

internal static class SuperFilterExtensions
{
    public static IQueryable<T> FilterProperty<T, TProperty>(
        this IQueryable<T> query,
        GlobalConfiguration globalConfiguration,
        Expression<Func<T, TProperty>> propertyExpression,
        bool isRequired)
    {
        Expression body = propertyExpression.Body is UnaryExpression unary ? unary.Operand : propertyExpression.Body;
        string propertyName = ((MemberExpression)body).Member.Name;

        KeyValuePair<string, FieldConfiguration> kvp = globalConfiguration.PropertyMappings
            .FirstOrDefault(x => x.Value.EntityPropertyName.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase));

        if (kvp.Equals(default(KeyValuePair<string, FieldConfiguration>)))
            return query;

        string? actualKey = kvp.Key;
        FieldConfiguration? fieldConfig = kvp.Value;


        string filterFieldName = propertyName;
        if (string.IsNullOrEmpty(fieldConfig!.EntityPropertyName))
            filterFieldName = propertyName;

        FilterCriterion? filter = globalConfiguration.HasFilters.Filters
            .FirstOrDefault(filters => string.Equals(filters.Field, actualKey, StringComparison.CurrentCultureIgnoreCase));

        if (filter == null || string.IsNullOrEmpty(filter.Value))
        {
            if (fieldConfig.IsRequired)
                throw new SuperFilterException($"Filter {filterFieldName} is required.");
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
        GlobalConfiguration globalConfiguration)
    {
        if (globalConfiguration.HasSorts.Sorters.Count == 0)
            throw new SuperFilterException("At least 1 sorter must be specified");

        foreach (SortCriterion sorter in globalConfiguration.HasSorts.Sorters)
        {
            if (!globalConfiguration.PropertyMappings.TryGetValue(sorter.Field, out FieldConfiguration? property)) continue;

            Expression body = property.Selector.Body is UnaryExpression unary ? unary.Operand : property.Selector.Body;

            if (body is not MemberExpression memberExpression)
                throw new InvalidOperationException($"Invalid expression for sorting: {property.Selector.Body}");

            ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
            Type propertyType = ((PropertyInfo)memberExpression.Member).PropertyType;

            Expression propertyAccess = GetNestedPropertyExpression(parameter, RemoveUntilFirstDot(memberExpression.ToString()));
            LambdaExpression? lambda = Expression.Lambda(Expression.Convert(propertyAccess, propertyType), parameter);

            string methodName = query.Expression.Type == typeof(IOrderedQueryable<T>)
                ? sorter.dir.Equals("asc", StringComparison.CurrentCultureIgnoreCase) ? "ThenBy" : "ThenByDescending"
                : sorter.dir.Equals("asc", StringComparison.CurrentCultureIgnoreCase)
                    ? "OrderBy"
                    : "OrderByDescending";

            MethodInfo? method = typeof(Queryable)
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
        int index = input.IndexOf('.');
        return index == -1 ? input : input.Substring(index + 1);
    }
}