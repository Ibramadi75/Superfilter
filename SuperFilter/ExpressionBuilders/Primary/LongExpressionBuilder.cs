using System.Linq.Expressions;
using Superfilter.Constants;

namespace Superfilter.ExpressionBuilders;

public static class LongExpressionBuilder
{
    public static Expression BuildLongFilterExpression(Expression property, string filterValue, Operator filterOperator)
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
        if (!long.TryParse(filterValue, out long longValue))
            throw new FormatException($"Invalid long format: {filterValue}");

        UnaryExpression constant = Expression.Convert(Expression.Constant(longValue), property.Type);

        Expression comparison = filterOperator switch
        {
            Operator.Equals => Expression.Equal(property, constant),
            Operator.NotEquals => Expression.NotEqual(property, constant),
            Operator.LessThan => Expression.LessThan(property, constant),
            Operator.LessThanOrEqual => Expression.LessThanOrEqual(property, constant),
            Operator.GreaterThan => Expression.GreaterThan(property, constant),
            Operator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(property, constant),
            _ => throw new InvalidOperationException($"Invalid operator {filterOperator} for long.")
        };

        // For nullable types, exclude null values from comparison operations
        if (property.Type == typeof(long?))
        {
            return Expression.AndAlso(
                Expression.NotEqual(property, Expression.Constant(null, property.Type)),
                comparison
            );
        }

        return comparison;
    }

    private static Expression BuildInExpression(Expression property, string filterValue)
    {
        string[] values = filterValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
        
        if (values.Length == 0)
            return Expression.Constant(false);

        Expression[] comparisons = values.Select(value =>
        {
            if (!long.TryParse(value.Trim(), out long longValue))
                throw new FormatException($"Invalid long format: {value}");
            return Expression.Equal(property, Expression.Convert(Expression.Constant(longValue), property.Type));
        }).ToArray();

        Expression inExpression = comparisons.Aggregate(Expression.OrElse);
        
        // For nullable types, exclude null values from In operations
        if (property.Type == typeof(long?))
        {
            return Expression.AndAlso(
                Expression.NotEqual(property, Expression.Constant(null, property.Type)),
                inExpression
            );
        }

        return inExpression;
    }

    private static Expression BuildBetweenExpression(Expression property, string filterValue)
    {
        string[] values = filterValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
        
        if (values.Length != 2)
            throw new ArgumentException("Between operator requires exactly two values separated by comma");

        if (!long.TryParse(values[0].Trim(), out long minValue) || !long.TryParse(values[1].Trim(), out long maxValue))
            throw new FormatException("Invalid long format in between values");

        UnaryExpression minConstant = Expression.Convert(Expression.Constant(minValue), property.Type);
        UnaryExpression maxConstant = Expression.Convert(Expression.Constant(maxValue), property.Type);

        Expression betweenExpression = Expression.AndAlso(
            Expression.GreaterThanOrEqual(property, minConstant),
            Expression.LessThanOrEqual(property, maxConstant)
        );
        
        // For nullable types, exclude null values from Between operations
        if (property.Type == typeof(long?))
        {
            return Expression.AndAlso(
                Expression.NotEqual(property, Expression.Constant(null, property.Type)),
                betweenExpression
            );
        }

        return betweenExpression;
    }
}