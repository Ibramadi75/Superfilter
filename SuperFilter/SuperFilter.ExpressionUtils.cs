using System.Linq.Expressions;
using System.Reflection;

namespace Superfilter;

public partial class Superfilter
{
    private static LambdaExpression CastSelectorToPropertyType<T>(LambdaExpression expression, Type targetType)
    {
        Expression body = expression.Body;

        // Retirer les conversions inutiles (Convert(x.Property, object))
        if (body is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Convert) body = unaryExpression.Operand;

        // Créer une nouvelle expression avec le type correct
        Type funcType = typeof(Func<,>).MakeGenericType(typeof(T), targetType);
        return Expression.Lambda(funcType, body, expression.Parameters);
    }
    private static Type ExtractPropertyTypeFromSelector(LambdaExpression selector)
    {
        Expression body = selector.Body is UnaryExpression unary ? unary.Operand : selector.Body;
        if (body is MemberExpression memberExpression) return ((PropertyInfo)memberExpression.Member).PropertyType;

        throw new SuperfilterException("Unable to determine property type from selector.");
    }

    private static Expression<Func<T, object>> NormalizeSelectorToObject<T>(LambdaExpression selector)
    {
        if (selector.Body.Type == typeof(string) ||
            selector.Body.Type == typeof(int) || selector.Body.Type == typeof(int?) ||
            selector.Body.Type == typeof(long) || selector.Body.Type == typeof(long?) ||
            selector.Body.Type == typeof(decimal) || selector.Body.Type == typeof(decimal?) ||
            selector.Body.Type == typeof(double) || selector.Body.Type == typeof(double?) ||
            selector.Body.Type == typeof(float) || selector.Body.Type == typeof(float?) ||
            selector.Body.Type == typeof(bool) ||
            selector.Body.Type == typeof(DateTime) || selector.Body.Type == typeof(DateTime?) ||
            selector.Body.Type == typeof(DateTimeOffset) || selector.Body.Type == typeof(DateTimeOffset?) ||
            selector.Body.Type == typeof(object))
            return Expression.Lambda<Func<T, object>>(Expression.Convert(selector.Body, typeof(object)), selector.Parameters);

        throw new SuperfilterException("Unsupported selector type for " + selector);
    }
    
    internal static LambdaExpression BuildSelectorLambda<T>(string memberExpression)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
        Expression propertyAccess = BuildNestedPropertyAccess(parameter, memberExpression);
        LambdaExpression lambda = Expression.Lambda(propertyAccess, parameter);
        return lambda;
    }

    private static Expression BuildNestedPropertyAccess(Expression parameter, string propertyPath)
    {
        return propertyPath.Split('.').Aggregate(parameter, Expression.Property);
    }
    
    /// <summary>
    /// Extracts the property path from a selector-like string by removing everything
    /// before the first dot ('.') and, if present, everything after the first comma (',').
    /// </summary>
    /// <param name="input">
    /// A string representing a selector, typically in the form of a lambda expression
    /// as a string (e.g., "x => x.User.Name" or "x => x.User.Name, String").
    /// </param>
    /// <returns>
    /// The substring after the first dot, truncated at the first comma if one exists.
    /// For example:
    /// - "x => x.User.Name" returns "User.Name"
    /// - "x => x.User.Name, String" returns "User.Name"
    /// - "x" returns "x"
    /// </returns>
    internal static string ExtractPropertyPathFromSelectorString(string input)
    {
        int index = input.IndexOf('.');
        if (index == -1) return input;

        string afterDot = input[(index + 1)..];
        int commaIndex = afterDot.IndexOf(',');

        return commaIndex == -1 ? afterDot : afterDot[..commaIndex];
    }
    
    /// <summary>
    /// Extracts the last word in a string that is separated by dots ('.').
    /// </summary>
    /// <param name="input">
    /// A string that may contain multiple words separated by dots.
    /// </param>
    /// <returns>
    /// The last word in the string after the last dot.
    /// "Database.Models.User" returns "User"
    /// </returns>
    internal static string ExtractLastWordFromDotSeparatedString(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        int lastDotIndex = input.LastIndexOf('.');
        if (lastDotIndex == -1) return input; // No dot found, return the whole string

        return input[(lastDotIndex + 1)..]; // Return substring after the last dot
    }
}