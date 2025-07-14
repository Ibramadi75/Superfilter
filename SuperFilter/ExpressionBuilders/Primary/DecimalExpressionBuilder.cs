using System.Globalization;
using System.Linq.Expressions;
using Superfilter.Constants;

namespace Superfilter.ExpressionBuilders;

public static class DecimalExpressionBuilder
{
    public static BinaryExpression BuildDecimalFilterExpression(Expression property, string filterValue, Operator filterOperator)
    {
        if (!decimal.TryParse(filterValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal decimalValue))
            throw new FormatException($"Invalid decimal format: {filterValue}");

        UnaryExpression constant = Expression.Convert(Expression.Constant(decimalValue), property.Type);

        return filterOperator switch
        {
            Operator.Equals => Expression.Equal(property, constant),
            Operator.LessThan => Expression.LessThan(property, constant),
            Operator.GreaterThan => Expression.GreaterThan(property, constant),
            _ => throw new InvalidOperationException("Invalid operator for decimal.")
        };
    }
}