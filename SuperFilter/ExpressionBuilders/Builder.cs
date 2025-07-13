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
            _ when typeof(T) == typeof(bool) || typeof(T) == typeof(bool?) => BoolExpressionBuilder.BuildBoolFilterExpression(property, filterValue),
            _ when typeof(T) == typeof(int) || typeof(T) == typeof(int?) => IntExpressionBuilder.BuildIntFilterExpression(property, filterValue, op),

            _ => throw new InvalidOperationException($"Unsupported type: {typeof(T).Name}")
        };

        if (methodCallExpression is null)
            throw new InvalidOperationException($"Invalid operator for type {typeof(T).Name}, must be one of: {MethodInfoMappings.GetMethodInfos(typeof(T))}");

        return methodCallExpression;
    }
}