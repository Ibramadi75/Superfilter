using System.Globalization;
using System.Linq.Expressions;
using Superfilter.Constants;
using Superfilter.ExpressionBuilders.Common;

namespace Superfilter.ExpressionBuilders;

public static class FloatExpressionBuilder
{
    public static Expression BuildFloatFilterExpression(Expression property, string filterValue, Operator filterOperator)
    {
        return CommonExpressionBuilder.BuildComplexFilterExpression(
            property, filterValue, filterOperator,
            (prop, value) => CommonExpressionBuilder.BuildInExpressionWithParser(prop, value, s => float.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture), "float"),
            (prop, value) => CommonExpressionBuilder.BuildBetweenExpressionWithParser(prop, value, s => float.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture), "float"),
            BuildComparisonExpression
        );
    }

    private static Expression BuildComparisonExpression(Expression property, string filterValue, Operator filterOperator)
    {
        if (!float.TryParse(filterValue, NumberStyles.Any, CultureInfo.InvariantCulture, out float floatValue))
            throw new FormatException($"Invalid float format: {filterValue}");

        UnaryExpression constant = Expression.Convert(Expression.Constant(floatValue), property.Type);

        return CommonExpressionBuilder.BuildComparisonExpression(property, constant, filterOperator);
    }

}