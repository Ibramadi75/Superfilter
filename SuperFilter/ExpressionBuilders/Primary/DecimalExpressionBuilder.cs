using System.Globalization;
using System.Linq.Expressions;
using Superfilter.Constants;

namespace Superfilter.ExpressionBuilders;

public static class DecimalExpressionBuilder
{
    public static Expression BuildDecimalFilterExpression(Expression property, string filterValue, Operator filterOperator)
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
        if (!decimal.TryParse(filterValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal decimalValue))
            throw new FormatException($"Invalid decimal format: {filterValue}");

        UnaryExpression constant = Expression.Convert(Expression.Constant(decimalValue), property.Type);

        return filterOperator switch
        {
            Operator.Equals => Expression.Equal(property, constant),
            Operator.NotEquals => Expression.NotEqual(property, constant),
            Operator.LessThan => Expression.LessThan(property, constant),
            Operator.LessThanOrEqual => Expression.LessThanOrEqual(property, constant),
            Operator.GreaterThan => Expression.GreaterThan(property, constant),
            Operator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(property, constant),
            _ => throw new InvalidOperationException($"Invalid operator {filterOperator} for decimal.")
        };
    }

    private static Expression BuildInExpression(Expression property, string filterValue)
    {
        string[] values = filterValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
        
        if (values.Length == 0)
            return Expression.Constant(false);

        Expression[] comparisons = values.Select(value =>
        {
            if (!decimal.TryParse(value.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal decimalValue))
                throw new FormatException($"Invalid decimal format: {value}");
            return Expression.Equal(property, Expression.Convert(Expression.Constant(decimalValue), property.Type));
        }).ToArray();

        return comparisons.Aggregate(Expression.OrElse);
    }

    private static Expression BuildBetweenExpression(Expression property, string filterValue)
    {
        string[] values = filterValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
        
        if (values.Length != 2)
            throw new ArgumentException("Between operator requires exactly two values separated by comma");

        if (!decimal.TryParse(values[0].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal minValue) || 
            !decimal.TryParse(values[1].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal maxValue))
            throw new FormatException("Invalid decimal format in between values");

        UnaryExpression minConstant = Expression.Convert(Expression.Constant(minValue), property.Type);
        UnaryExpression maxConstant = Expression.Convert(Expression.Constant(maxValue), property.Type);

        return Expression.AndAlso(
            Expression.GreaterThanOrEqual(property, minConstant),
            Expression.LessThanOrEqual(property, maxConstant)
        );
    }
}