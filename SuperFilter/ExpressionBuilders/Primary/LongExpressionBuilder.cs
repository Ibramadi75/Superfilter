using System.Linq.Expressions;
using Superfilter.Constants;

namespace Superfilter.ExpressionBuilders;

public static class LongExpressionBuilder
{
    public static BinaryExpression BuildLongFilterExpression(Expression property, string filterValue, Operator filterOperator)
    {
        if (!long.TryParse(filterValue, out long longValue))
            throw new FormatException($"Invalid long format: {filterValue}");

        UnaryExpression constant = Expression.Convert(Expression.Constant(longValue), property.Type);

        return filterOperator switch
        {
            Operator.Equals => Expression.Equal(property, constant),
            Operator.LessThan => Expression.LessThan(property, constant),
            Operator.GreaterThan => Expression.GreaterThan(property, constant),
            _ => throw new InvalidOperationException("Invalid operator for long.")
        };
    }
}