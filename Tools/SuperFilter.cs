using System.Linq.Expressions;
using System.Reflection;

namespace SuperFilter
{
    public class SuperFilter
    {
        private GlobalConfiguration? GlobalConfiguration { get; set; } = null;
        private Dictionary<Type, object> FieldConfigurations { get; set; } = new();

        public IQueryable<T> ApplyFilters<T>(IQueryable<T> query)
        {
            var fieldConfiguration = GetFieldConfiguration<T>();
            if (fieldConfiguration.Count == 0) throw new SuperFilterException($"No configuration found for {typeof(T).Name}");

            if (GlobalConfiguration == null) throw new SuperFilterException($"No global configuration found");
            
            CheckRequiredFilters();

            foreach (var filter in GlobalConfiguration.HasFilters.Filters)
            {
                if (GlobalConfiguration.PropertyMappings.TryGetValue(filter.Field, out var fieldConfig) && fieldConfig.Selector != null)
                {
                    var typedSelector = ConvertSelector<T>(fieldConfig.Selector);
                    var propertyType = GetSelectorPropertyType(fieldConfig.Selector);

                    var typedExpression = ConvertExpressionType<T>(typedSelector, propertyType); // Corrigé

                    var filterMethod = typeof(SuperFilterExtensions)
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
            }

            return query;
        }
        
        public static LambdaExpression ConvertExpressionType<T>(LambdaExpression expression, Type targetType)
        {
            Expression body = expression.Body;

            // Retirer les conversions inutiles (Convert(x.Property, object))
            if (body is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Convert)
            {
                body = unaryExpression.Operand;
            }

            // Créer une nouvelle expression avec le type correct
            Type funcType = typeof(Func<,>).MakeGenericType(typeof(T), targetType);
            return Expression.Lambda(funcType, body, expression.Parameters);
        }

        private void CheckRequiredFilters()
        {
            foreach (var propertyMapping in GlobalConfiguration.PropertyMappings)
            {
                var fieldConfig = propertyMapping.Value;
                if (fieldConfig.IsRequired && GlobalConfiguration.HasFilters.Filters.All(f => f.Field != propertyMapping.Key))
                {
                    throw new SuperFilterException($"Filter {propertyMapping.Key} is required.");
                }
            }
        }
        
        private Type GetSelectorPropertyType(LambdaExpression selector)
        {
            var body = selector.Body is UnaryExpression unary ? unary.Operand : selector.Body;

            if (body is MemberExpression memberExpression)
            {
                return ((PropertyInfo)memberExpression.Member).PropertyType;
            }

            throw new SuperFilterException("Unable to determine property type from selector.");
        }


        public static Expression<Func<T, object>> ConvertSelector<T>(LambdaExpression selector)
        {
            if (selector.Body.Type == typeof(string) || (selector.Body.Type == typeof(int) || selector.Body.Type == typeof(bool) || selector.Body.Type == typeof(object)))
            {
                return Expression.Lambda<Func<T, object>>(Expression.Convert(selector.Body, typeof(object)), selector.Parameters);
            }
            else
            {
                throw new SuperFilterException("Unsupported selector type for " + selector);
            }
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

            var fieldConfigDict = FieldConfigurations[typeof(T)] as Dictionary<string, FieldConfiguration>;

            foreach (var mapping in GlobalConfiguration.PropertyMappings)
            {
                var fieldConfig = mapping.Value;
                
                try
                {
                    fieldConfig.Selector = CreateSelectorExpression<T>(mapping.Key);
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

        private LambdaExpression CreateSelectorExpression<T>(string fieldName)
        {
            var param = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(param, fieldName);
            var lambda = Expression.Lambda(property, param);
            return lambda;
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
        public string DtoPropertyName { get; set; } = string.Empty;
        public bool IsRequired { get; set; } = false;
        public LambdaExpression Selector { get; set; }
    }

    public enum FilterErrorStrategy
    {
        ThrowException,
        Ignore,
        ApplyDefault
    }
}