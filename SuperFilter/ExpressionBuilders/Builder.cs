using System.Linq.Expressions;
using Superfilter.Constants;

namespace Superfilter.ExpressionBuilders;

public static class Builder
{
    public static Expression GetExpression<T>(MemberExpression property, string filterValue, Operator op)
    {
        Expression? methodCallExpression = op switch
        {
            _ when typeof(T) == typeof(string) => StringExpressionBuilder.BuildStringFilterExpression(property, filterValue, op),
            _ when typeof(T) == typeof(DateTime) || typeof(T) == typeof(DateTime?) => DateExpressionBuilder.BuildDateFilterExpression(property, filterValue, op),
            _ when typeof(T) == typeof(DateTimeOffset) || typeof(T) == typeof(DateTimeOffset?) => DateExpressionBuilder.BuildDateTimeOffsetFilterExpression(property, filterValue, op),
            _ when typeof(T) == typeof(bool) || typeof(T) == typeof(bool?) => BoolExpressionBuilder.BuildBoolFilterExpression(property, filterValue, op),
            _ when typeof(T) == typeof(int) || typeof(T) == typeof(int?) => IntExpressionBuilder.BuildIntFilterExpression(property, filterValue, op),
            _ when typeof(T) == typeof(long) || typeof(T) == typeof(long?) => LongExpressionBuilder.BuildLongFilterExpression(property, filterValue, op),
            _ when typeof(T) == typeof(decimal) || typeof(T) == typeof(decimal?) => DecimalExpressionBuilder.BuildDecimalFilterExpression(property, filterValue, op),
            _ when typeof(T) == typeof(double) || typeof(T) == typeof(double?) => DoubleExpressionBuilder.BuildDoubleFilterExpression(property, filterValue, op),
            _ when typeof(T) == typeof(float) || typeof(T) == typeof(float?) => FloatExpressionBuilder.BuildFloatFilterExpression(property, filterValue, op),

            _ => throw new InvalidOperationException($"Unsupported type: {typeof(T).Name}")
        };

        if (methodCallExpression is null)
            throw new InvalidOperationException($"Invalid operator for type {typeof(T).Name}, must be one of: {MethodInfoMappings.GetMethodInfos(typeof(T))}");

        return methodCallExpression;
    }
}