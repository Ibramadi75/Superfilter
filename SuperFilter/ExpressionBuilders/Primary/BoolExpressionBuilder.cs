using System.Linq.Expressions;
using Superfilter.Constants;

namespace Superfilter.ExpressionBuilders;

public static class BoolExpressionBuilder
{
    public static Expression BuildBoolFilterExpression(Expression property, string filterValue, Operator filterOperator)
    {
        return filterOperator switch
        {
            Operator.IsNull => Expression.Equal(property, Expression.Constant(null, property.Type)),
            Operator.IsNotNull => Expression.NotEqual(property, Expression.Constant(null, property.Type)),
            _ => BuildComparisonExpression(property, filterValue, filterOperator)
        };
    }

    public static Expression BuildBoolFilterExpression(Expression property, string filterValue)
    {
        return BuildBoolFilterExpression(property, filterValue, Operator.Equals);
    }

    private static Expression BuildComparisonExpression(Expression property, string filterValue, Operator filterOperator)
    {
        if (!bool.TryParse(filterValue, out bool boolValue))
            throw new FormatException($"Invalid boolean format: {filterValue}");

        UnaryExpression constant = Expression.Convert(Expression.Constant(boolValue), property.Type);

        return filterOperator switch
        {
            Operator.Equals => Expression.Equal(property, constant),
            Operator.NotEquals => Expression.NotEqual(property, constant),
            _ => throw new InvalidOperationException($"Invalid operator {filterOperator} for boolean.")
        };
    }
}