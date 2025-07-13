using System.Linq.Expressions;
using System.Reflection;
using Superfilter.Entities;

namespace Superfilter;

public partial class Superfilter
{
    private GlobalConfiguration? GlobalConfiguration { get; set; }
    private Dictionary<Type, object> FieldConfigurations { get; } = new();

    public IQueryable<T> ApplyConfiguredFilters<T>(IQueryable<T> query)
    {
        Dictionary<string, FieldConfiguration>? fieldConfiguration = GetFieldConfigurationsForType<T>();
        if (fieldConfiguration is { Count: 0 }) throw new SuperfilterException($"No configuration found for {typeof(T).Name}");

        if (GlobalConfiguration == null) throw new SuperfilterException("No global configuration found");

        ValidateRequiredFiltersPresence();
        
        if (GlobalConfiguration.HasFilters.Filters.Count == 0)
            return query;

        foreach (FilterCriterion filter in GlobalConfiguration.HasFilters.Filters)
            if (GlobalConfiguration.PropertyMappings.TryGetValue(filter.Field, out FieldConfiguration? fieldConfig))
            {
                Expression<Func<T, object>> typedSelector = NormalizeSelectorToObject<T>(fieldConfig.Selector);
                Type propertyType = ExtractPropertyTypeFromSelector(fieldConfig.Selector);

                LambdaExpression typedExpression = CastSelectorToPropertyType<T>(typedSelector, propertyType);

                MethodInfo? filterMethod = typeof(SuperfilterExtensions)
                    .GetMethod("FilterProperty")
                    ?.MakeGenericMethod(typeof(T), propertyType);

                try
                {
                    query = (IQueryable<T>)filterMethod?.Invoke(this, [query, GlobalConfiguration, typedExpression, fieldConfig.IsRequired])!;
                }
                catch (Exception e)
                {
                    throw new SuperfilterException($"Problem occurred while invoking SuperfilterExtensions.FilterProperty on {filter.Field} with operator {filter.Operator}.\n", e);
                }
            }

        return query;
    }
    public void InitializeGlobalConfiguration(GlobalConfiguration globalConfiguration)
    {
        if (GlobalConfiguration != null)
            throw new SuperfilterException("GlobalConfiguration is already set. Use a new instance of Superfilter for different configurations.\n Using the same instance will lead to unexpected behavior.");
        GlobalConfiguration = globalConfiguration;
    }
    public void InitializeFieldSelectors<T>()
    {
        if (GlobalConfiguration is null)
            throw new SuperfilterException("GlobalConfiguration must be set before using AddFieldConfiguration");

        if (!FieldConfigurations.ContainsKey(typeof(T)))
            FieldConfigurations[typeof(T)] = new Dictionary<string, FieldConfiguration>();

        Dictionary<string, FieldConfiguration>? fieldConfigDict = FieldConfigurations[typeof(T)] as Dictionary<string, FieldConfiguration>;

        foreach (KeyValuePair<string, FieldConfiguration> mapping in GlobalConfiguration.PropertyMappings)
        {
            FieldConfiguration fieldConfig = mapping.Value;

            // Si l'expression retourne object, on doit la reconstruire avec le type correct
            if (fieldConfig.Selector.Body.Type == typeof(object) && fieldConfig.Selector.Body is UnaryExpression unary && unary.NodeType == ExpressionType.Convert)
            {
                try
                {
                    string propertyPath = ExtractPropertyPathFromSelectorString(fieldConfig.Selector.ToString());
                    fieldConfig.Selector = BuildSelectorLambda<T>(propertyPath);
                }
                catch
                {
                    // Si la reconstruction échoue, on garde l'expression originale
                }
            }

            if (fieldConfigDict != null) fieldConfigDict[mapping.Key] = fieldConfig;
        }
    }
}
