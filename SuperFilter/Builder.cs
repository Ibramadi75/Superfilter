using System.Linq.Expressions;
using System.Reflection;
using SuperFilter.Constants;

namespace SuperFilter;

public static class Builder
{
    public static Expression GetExpression<T>(MemberExpression property, string filterValue, Operator op)
    {
        Expression? methodCallExpression = op switch
        {
            _ when typeof(T) == typeof(string) => BuildStringFilterExpression(property, filterValue, op),
            _ when typeof(T) == typeof(DateTime) || typeof(T) == typeof(DateTime?) => BuildDateFilterExpression(property, filterValue, op),
            _ when typeof(T) == typeof(bool) || typeof(T) == typeof(bool?) => BuildBoolFilterExpression(property, filterValue, op),
            _ when typeof(T) == typeof(int) || typeof(T) == typeof(int?) => BuildIntFilterExpression(property, filterValue, op),

            _ => throw new InvalidOperationException($"Unsupported type: {typeof(T).Name}")
        };

        if (methodCallExpression is null)
            throw new InvalidOperationException($"Invalid operator for type {typeof(T).Name}, must be one of: {MethodInfoMappings.GetMethodInfos(typeof(T))}");

        return methodCallExpression;
    }

    private static MethodCallExpression? BuildStringFilterExpression(Expression property, string filterValue, Operator op)
    {
        MethodInfo? method = MethodInfoMappings.GetMethodInfos(typeof(string))[op];
        return method is null ? null : Expression.Call(property, method, Expression.Constant(filterValue));
    }

    private static BinaryExpression BuildDateFilterExpression(Expression property, string filterValue, Operator operatorName)
    {
        if (!DateTime.TryParse(filterValue, out DateTime filterDate))
            throw new FormatException($"Invalid date format: {filterValue}");

        Expression propertyValue = property.Type == typeof(DateTime?)
            ? Expression.Coalesce(property, Expression.Constant(DateTime.MinValue))
            : property;

        return operatorName switch
        {
            Operator.Equals => Expression.Equal(propertyValue, Expression.Constant(filterDate)),
            Operator.IsEqualToFullDate => Expression.AndAlso(
                Expression.Equal(Expression.Property(propertyValue, nameof(DateTime.Year)), Expression.Constant(filterDate.Year)),
                Expression.AndAlso(
                    Expression.Equal(Expression.Property(propertyValue, nameof(DateTime.Month)), Expression.Constant(filterDate.Month)),
                    Expression.Equal(Expression.Property(propertyValue, nameof(DateTime.Day)), Expression.Constant(filterDate.Day))
                )
            ),
            Operator.IsEqualToYearAndMonth => Expression.AndAlso(
                Expression.Equal(Expression.Property(propertyValue, nameof(DateTime.Year)), Expression.Constant(filterDate.Year)),
                Expression.Equal(Expression.Property(propertyValue, nameof(DateTime.Month)), Expression.Constant(filterDate.Month))
            ),
            Operator.IsEqualToYear => Expression.Equal(Expression.Property(propertyValue, nameof(DateTime.Year)), Expression.Constant(filterDate.Year)),

            Operator.LessThan => Expression.LessThan(propertyValue, Expression.Constant(filterDate)),
            Operator.GreaterThan => Expression.GreaterThan(propertyValue, Expression.Constant(filterDate)),

            _ => throw new InvalidOperationException("Invalid operator for date.")
        };
    }

    private static BinaryExpression BuildBoolFilterExpression(Expression property, string filterValue, Operator filterOperator)
    {
        if (!bool.TryParse(filterValue, out bool boolValue))
            throw new FormatException($"Invalid boolean format: {filterValue}");

        return Expression.Equal(property, Expression.Constant(boolValue));
    }

    private static BinaryExpression BuildIntFilterExpression(Expression property, string filterValue, Operator filterOperator)
    {
        if (!int.TryParse(filterValue, out int intValue))
            throw new FormatException($"Invalid integer format: {filterValue}");

        UnaryExpression constant = Expression.Convert(Expression.Constant(intValue), property.Type);

        return filterOperator switch
        {
            Operator.Equals => Expression.Equal(property, constant),
            Operator.LessThan => Expression.LessThan(property, constant),
            Operator.GreaterThan => Expression.GreaterThan(property, constant),
            _ => throw new InvalidOperationException("Invalid operator for integer.")
        };
    }
}