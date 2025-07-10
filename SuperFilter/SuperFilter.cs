using System.Linq.Expressions;
using System.Reflection;

namespace SuperFilter;

public class SuperFilter
{
    private GlobalConfiguration? GlobalConfiguration { get; set; }
    private Dictionary<Type, object> FieldConfigurations { get; } = new();

    public IQueryable<T> ApplyFilters<T>(IQueryable<T> query)
    {
        Dictionary<string, FieldConfiguration> fieldConfiguration = GetFieldConfiguration<T>();
        if (fieldConfiguration.Count == 0) throw new SuperFilterException($"No configuration found for {typeof(T).Name}");

        if (GlobalConfiguration == null) throw new SuperFilterException("No global configuration found");

        CheckRequiredFilters();

        foreach (FilterCriterion filter in GlobalConfiguration.HasFilters.Filters)
            if (GlobalConfiguration.PropertyMappings.TryGetValue(filter.Field, out FieldConfiguration? fieldConfig) && fieldConfig.Selector != null)
            {
                Expression<Func<T, object>> typedSelector = ConvertSelector<T>(fieldConfig.Selector);
                Type propertyType = GetSelectorPropertyType(fieldConfig.Selector);

                LambdaExpression typedExpression = ConvertExpressionType<T>(typedSelector, propertyType);

                MethodInfo? filterMethod = typeof(SuperFilterExtensions)
                    .GetMethod("FilterProperty")
                    ?.MakeGenericMethod(typeof(T), propertyType);

                try
                {
                    query = (IQueryable<T>)filterMethod?.Invoke(this, [query, GlobalConfiguration, typedExpression, fieldConfig.IsRequired])!;
                }
                catch (Exception e)
                {
                    throw new SuperFilterException($"Problem occurred while invoking SuperFilterExtensions.FilterProperty on {filter.Field} with operator {filter.Operator}");
                }
            }

        return query;
    }

    public static LambdaExpression ConvertExpressionType<T>(LambdaExpression expression, Type targetType)
    {
        Expression body = expression.Body;

        // Retirer les conversions inutiles (Convert(x.Property, object))
        if (body is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Convert) body = unaryExpression.Operand;

        // Créer une nouvelle expression avec le type correct
        Type funcType = typeof(Func<,>).MakeGenericType(typeof(T), targetType);
        return Expression.Lambda(funcType, body, expression.Parameters);
    }

    private void CheckRequiredFilters()
    {
        foreach (KeyValuePair<string, FieldConfiguration> propertyMapping in GlobalConfiguration.PropertyMappings)
        {
            FieldConfiguration fieldConfig = propertyMapping.Value;
            if (fieldConfig.IsRequired && GlobalConfiguration.HasFilters.Filters.All(f => f.Field != propertyMapping.Key)) throw new SuperFilterException($"Filter {propertyMapping.Key} is required.");
        }
    }

    private Type GetSelectorPropertyType(LambdaExpression selector)
    {
        Expression body = selector.Body is UnaryExpression unary ? unary.Operand : selector.Body;

        if (body is MemberExpression memberExpression) return ((PropertyInfo)memberExpression.Member).PropertyType;

        throw new SuperFilterException("Unable to determine property type from selector.");
    }

    public static Expression<Func<T, object>> ConvertSelector<T>(LambdaExpression selector)
    {
        if (selector.Body.Type == typeof(string) ||
            selector.Body.Type == typeof(int) || selector.Body.Type == typeof(int?) ||
            selector.Body.Type == typeof(bool) ||
            selector.Body.Type == typeof(DateTime) || selector.Body.Type == typeof(DateTime?) ||
            selector.Body.Type == typeof(object))
            return Expression.Lambda<Func<T, object>>(Expression.Convert(selector.Body, typeof(object)), selector.Parameters);

        throw new SuperFilterException("Unsupported selector type for " + selector);
    }

    public void SetGlobalConfiguration(GlobalConfiguration globalConfiguration)
    {
        GlobalConfiguration = globalConfiguration;
    }

    public void SetupFieldConfiguration<T>()
    {
        if (GlobalConfiguration is null)
            throw new SuperFilterException("GlobalConfiguration must be set before using AddFieldConfiguration");
        if (GlobalConfiguration.PropertyMappings == null) return;

        if (!FieldConfigurations.ContainsKey(typeof(T)))
            FieldConfigurations[typeof(T)] = new Dictionary<string, FieldConfiguration>();

        Dictionary<string, FieldConfiguration>? fieldConfigDict = FieldConfigurations[typeof(T)] as Dictionary<string, FieldConfiguration>;

        foreach (KeyValuePair<string, FieldConfiguration> mapping in GlobalConfiguration.PropertyMappings)
        {
            FieldConfiguration fieldConfig = mapping.Value;

            if (fieldConfig.Selector.ToString().Count(c => c == '.') > 1)
                try
                {
                    fieldConfig.Selector = CreateSelectorExpression<T>(ExtractExpression(fieldConfig.Selector.ToString()));
                }
                catch
                {
                    continue;
                }
            else
                try
                {
                    fieldConfig.Selector = CreateSelectorExpression<T>(mapping.Value.EntityPropertyName);
                }
                catch
                {
                    continue;
                }

            if (fieldConfigDict != null) fieldConfigDict[mapping.Key] = fieldConfig;
        }
    }


    private Dictionary<string, FieldConfiguration> GetFieldConfiguration<T>()
    {
        if (FieldConfigurations.ContainsKey(typeof(T)))
            return FieldConfigurations[typeof(T)] as Dictionary<string, FieldConfiguration>;

        return new Dictionary<string, FieldConfiguration>();
    }

    private LambdaExpression CreateSelectorExpression<T>(string memberExpression)
    {
        ParameterExpression? parameter = Expression.Parameter(typeof(T), "x");
        Expression? propertyAccess = GetNestedPropertyExpression(parameter, memberExpression); // Corrigé
        LambdaExpression? lambda = Expression.Lambda(propertyAccess, parameter); // Corrigé
        return lambda;
    }

    private static Expression GetNestedPropertyExpression(Expression parameter, string propertyPath)
    {
        Expression propertyAccess = parameter;
        foreach (string? property in propertyPath.Split('.')) propertyAccess = Expression.Property(propertyAccess, property);
        return propertyAccess;
    }

    private static string ExtractExpression(string input)
    {
        int index = input.IndexOf('.');
        if (index == -1) return input;

        string afterDot = input.Substring(index + 1);
        int commaIndex = afterDot.IndexOf(',');

        return commaIndex == -1 ? afterDot : afterDot.Substring(0, commaIndex);
    }
}

public class GlobalConfiguration
{
    public Dictionary<string, FieldConfiguration> PropertyMappings { get; set; } = new();
    public FilterErrorStrategy MissingFilterStrategy { get; set; } = FilterErrorStrategy.ThrowException;
    public Exception StandardException { get; set; } = new SuperFilterException();
    public IHasFilters HasFilters { get; set; }
    public IHasSorts HasSorts { get; set; }
}

public class FieldConfiguration
{
    public string EntityPropertyName { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = false;
    public LambdaExpression Selector { get; set; }
}

public enum FilterErrorStrategy
{
    ThrowException,
    Ignore,
    ApplyDefault
}