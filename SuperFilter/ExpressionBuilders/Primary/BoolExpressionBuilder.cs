using System.Linq.Expressions;

namespace Superfilter.ExpressionBuilders;

public static class BoolExpressionBuilder
{
    public static BinaryExpression BuildBoolFilterExpression(Expression property, string filterValue)
    {
        if (!bool.TryParse(filterValue, out bool boolValue))
            throw new FormatException($"Invalid boolean format: {filterValue}");

        return Expression.Equal(property, Expression.Constant(boolValue));
    }
}