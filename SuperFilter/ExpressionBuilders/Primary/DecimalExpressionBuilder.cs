using System.Globalization;
using System.Linq.Expressions;
using Superfilter.Constants;
using Superfilter.ExpressionBuilders.Common;

namespace Superfilter.ExpressionBuilders;

public static class DecimalExpressionBuilder
{
    public static Expression BuildDecimalFilterExpression(Expression property, string filterValue, Operator filterOperator)
    {
        return CommonExpressionBuilder.BuildComplexFilterExpression(
            property, filterValue, filterOperator,
            (prop, value) => CommonExpressionBuilder.BuildInExpressionWithParser(prop, value, s => decimal.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture), "decimal"),
            (prop, value) => CommonExpressionBuilder.BuildBetweenExpressionWithParser(prop, value, s => decimal.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture), "decimal"),
            BuildComparisonExpression
        );
    }

    private static Expression BuildComparisonExpression(Expression property, string filterValue, Operator filterOperator)
    {
        if (!decimal.TryParse(filterValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal decimalValue))
            throw new FormatException($"Invalid decimal format: {filterValue}");

        UnaryExpression constant = Expression.Convert(Expression.Constant(decimalValue), property.Type);

        return CommonExpressionBuilder.BuildComparisonExpression(property, constant, filterOperator);
    }

}