using System.Linq.Expressions;
using Superfilter.Constants;

namespace Superfilter.ExpressionBuilders;

public static class IntExpressionBuilder
{
    public static Expression BuildIntFilterExpression(Expression property, string filterValue, Operator filterOperator)
    {
        return filterOperator switch
        {
            Operator.IsNull => Expression.Equal(property, Expression.Constant(null, property.Type)),
            Operator.IsNotNull => Expression.NotEqual(property, Expression.Constant(null, property.Type)),
            Operator.In => BuildInExpression(property, filterValue),
            Operator.NotIn => Expression.Not(BuildInExpression(property, filterValue)),
            Operator.Between => BuildBetweenExpression(property, filterValue),
            Operator.NotBetween => Expression.Not(BuildBetweenExpression(property, filterValue)),
            _ => BuildComparisonExpression(property, filterValue, filterOperator)
        };
    }

    private static Expression BuildComparisonExpression(Expression property, string filterValue, Operator filterOperator)
    {
        if (!int.TryParse(filterValue, out int intValue))
            throw new FormatException($"Invalid integer format: {filterValue}");

        UnaryExpression constant = Expression.Convert(Expression.Constant(intValue), property.Type);

        return filterOperator switch
        {
            Operator.Equals => Expression.Equal(property, constant),
            Operator.NotEquals => Expression.NotEqual(property, constant),
            Operator.LessThan => Expression.LessThan(property, constant),
            Operator.LessThanOrEqual => Expression.LessThanOrEqual(property, constant),
            Operator.GreaterThan => Expression.GreaterThan(property, constant),
            Operator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(property, constant),
            _ => throw new InvalidOperationException($"Invalid operator {filterOperator} for integer.")
        };
    }

    private static Expression BuildInExpression(Expression property, string filterValue)
    {
        string[] values = filterValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
        
        if (values.Length == 0)
            return Expression.Constant(false);

        Expression[] comparisons = values.Select(value =>
        {
            if (!int.TryParse(value.Trim(), out int intValue))
                throw new FormatException($"Invalid integer format: {value}");
            return Expression.Equal(property, Expression.Convert(Expression.Constant(intValue), property.Type));
        }).ToArray();

        return comparisons.Aggregate(Expression.OrElse);
    }

    private static Expression BuildBetweenExpression(Expression property, string filterValue)
    {
        string[] values = filterValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
        
        if (values.Length != 2)
            throw new ArgumentException("Between operator requires exactly two values separated by comma");

        if (!int.TryParse(values[0].Trim(), out int minValue) || !int.TryParse(values[1].Trim(), out int maxValue))
            throw new FormatException("Invalid integer format in between values");

        UnaryExpression minConstant = Expression.Convert(Expression.Constant(minValue), property.Type);
        UnaryExpression maxConstant = Expression.Convert(Expression.Constant(maxValue), property.Type);

        return Expression.AndAlso(
            Expression.GreaterThanOrEqual(property, minConstant),
            Expression.LessThanOrEqual(property, maxConstant)
        );
    }
}