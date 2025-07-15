using System.Globalization;
using System.Linq.Expressions;
using Superfilter.Constants;

namespace Superfilter.ExpressionBuilders;

public static class DoubleExpressionBuilder
{
    public static BinaryExpression BuildDoubleFilterExpression(Expression property, string filterValue, Operator filterOperator)
    {
        if (!double.TryParse(filterValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double doubleValue))
            throw new FormatException($"Invalid double format: {filterValue}");

        UnaryExpression constant = Expression.Convert(Expression.Constant(doubleValue), property.Type);

        return filterOperator switch
        {
            Operator.Equals => Expression.Equal(property, constant),
            Operator.LessThan => Expression.LessThan(property, constant),
            Operator.GreaterThan => Expression.GreaterThan(property, constant),
            _ => throw new InvalidOperationException("Invalid operator for double.")
        };
    }
}