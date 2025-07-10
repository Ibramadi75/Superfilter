using System.Linq.Expressions;
using System.Reflection;

namespace SuperFilter;

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

        throw new SuperFilterException("Unable to determine property type from selector.");
    }

    private static Expression<Func<T, object>> NormalizeSelectorToObject<T>(LambdaExpression selector)
    {
        if (selector.Body.Type == typeof(string) ||
            selector.Body.Type == typeof(int) || selector.Body.Type == typeof(int?) ||
            selector.Body.Type == typeof(bool) ||
            selector.Body.Type == typeof(DateTime) || selector.Body.Type == typeof(DateTime?) ||
            selector.Body.Type == typeof(object))
            return Expression.Lambda<Func<T, object>>(Expression.Convert(selector.Body, typeof(object)), selector.Parameters);

        throw new SuperFilterException("Unsupported selector type for " + selector);
    }
    
    private static LambdaExpression BuildSelectorLambda<T>(string memberExpression)
    {
        ParameterExpression? parameter = Expression.Parameter(typeof(T), "x");
        Expression? propertyAccess = BuildNestedPropertyAccess(parameter, memberExpression); // Corrigé
        LambdaExpression? lambda = Expression.Lambda(propertyAccess, parameter); // Corrigé
        return lambda;
    }

    private static Expression BuildNestedPropertyAccess(Expression parameter, string propertyPath)
    {
        Expression propertyAccess = parameter;
        foreach (string? property in propertyPath.Split('.')) propertyAccess = Expression.Property(propertyAccess, property);
        return propertyAccess;
    }

    private static string ExtractPropertyPathFromSelectorString(string input)
    {
        int index = input.IndexOf('.');
        if (index == -1) return input;

        string afterDot = input.Substring(index + 1);
        int commaIndex = afterDot.IndexOf(',');

        return commaIndex == -1 ? afterDot : afterDot[..commaIndex];
    }
}