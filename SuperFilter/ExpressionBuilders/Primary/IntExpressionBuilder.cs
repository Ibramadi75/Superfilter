using System.Linq.Expressions;
using Superfilter.Constants;

namespace Superfilter.ExpressionBuilders;

public static class IntExpressionBuilder
{
    public static BinaryExpression BuildIntFilterExpression(Expression property, string filterValue, Operator filterOperator)
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