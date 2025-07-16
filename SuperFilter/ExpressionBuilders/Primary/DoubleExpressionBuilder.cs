using System.Globalization;
using System.Linq.Expressions;
using Superfilter.Constants;
using Superfilter.ExpressionBuilders.Common;

namespace Superfilter.ExpressionBuilders;

public static class DoubleExpressionBuilder
{
    public static Expression BuildDoubleFilterExpression(Expression property, string filterValue, Operator filterOperator)
    {
        return CommonExpressionBuilder.BuildComplexFilterExpression(
            property, filterValue, filterOperator,
            (prop, value) => CommonExpressionBuilder.BuildInExpressionWithParser(prop, value, s => double.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture), "double"),
            (prop, value) => CommonExpressionBuilder.BuildBetweenExpressionWithParser(prop, value, s => double.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture), "double"),
            BuildComparisonExpression
        );
    }

    private static Expression BuildComparisonExpression(Expression property, string filterValue, Operator filterOperator)
    {
        if (!double.TryParse(filterValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double doubleValue))
            throw new FormatException($"Invalid double format: {filterValue}");

        UnaryExpression constant = Expression.Convert(Expression.Constant(doubleValue), property.Type);

        return CommonExpressionBuilder.BuildComparisonExpression(property, constant, filterOperator);
    }

}