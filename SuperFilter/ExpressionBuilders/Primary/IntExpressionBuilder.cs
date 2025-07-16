using System.Linq.Expressions;
using Superfilter.Constants;
using Superfilter.ExpressionBuilders.Common;

namespace Superfilter.ExpressionBuilders;

public static class IntExpressionBuilder
{
    public static Expression BuildIntFilterExpression(Expression property, string filterValue, Operator filterOperator)
    {
        return CommonExpressionBuilder.BuildComplexFilterExpression(
            property, filterValue, filterOperator,
            (prop, value) => CommonExpressionBuilder.BuildInExpressionWithParser(prop, value, int.Parse, "integer"),
            (prop, value) => CommonExpressionBuilder.BuildBetweenExpressionWithParser(prop, value, int.Parse, "integer"),
            BuildComparisonExpression
        );
    }

    private static Expression BuildComparisonExpression(Expression property, string filterValue, Operator filterOperator)
    {
        if (!int.TryParse(filterValue, out int intValue))
            throw new FormatException($"Invalid integer format: {filterValue}");

        UnaryExpression constant = Expression.Convert(Expression.Constant(intValue), property.Type);

        return CommonExpressionBuilder.BuildComparisonExpression(property, constant, filterOperator);
    }

}