using System.Globalization;
using System.Linq.Expressions;
using Superfilter.Constants;

namespace Superfilter.ExpressionBuilders;

public static class FloatExpressionBuilder
{
    public static BinaryExpression BuildFloatFilterExpression(Expression property, string filterValue, Operator filterOperator)
    {
        if (!float.TryParse(filterValue, NumberStyles.Any, CultureInfo.InvariantCulture, out float floatValue))
            throw new FormatException($"Invalid float format: {filterValue}");

        UnaryExpression constant = Expression.Convert(Expression.Constant(floatValue), property.Type);

        return filterOperator switch
        {
            Operator.Equals => Expression.Equal(property, constant),
            Operator.LessThan => Expression.LessThan(property, constant),
            Operator.GreaterThan => Expression.GreaterThan(property, constant),
            _ => throw new InvalidOperationException("Invalid operator for float.")
        };
    }
}