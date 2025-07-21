using System.Linq.Expressions;
using Superfilter.Constants;
using Superfilter.ExpressionBuilders.Common;

namespace Superfilter.ExpressionBuilders;

public static class LongExpressionBuilder
{
    public static Expression BuildLongFilterExpression(Expression property, string filterValue, Operator filterOperator)
    {
        return CommonExpressionBuilder.BuildComplexFilterExpression(
            property, filterValue, filterOperator,
            (prop, value) => CommonExpressionBuilder.WrapWithNullCheck(prop, CommonExpressionBuilder.BuildInExpressionWithParser(prop, value, long.Parse, "long")),
            (prop, value) => CommonExpressionBuilder.WrapWithNullCheck(prop, CommonExpressionBuilder.BuildBetweenExpressionWithParser(prop, value, long.Parse, "long")),
            BuildComparisonExpression
        );
    }

    private static Expression BuildComparisonExpression(Expression property, string filterValue, Operator filterOperator)
    {
        if (!long.TryParse(filterValue, out long longValue))
            throw new FormatException($"Invalid long format: {filterValue}");

        UnaryExpression constant = Expression.Convert(Expression.Constant(longValue), property.Type);

        Expression comparison = CommonExpressionBuilder.BuildComparisonExpression(property, constant, filterOperator);
        return CommonExpressionBuilder.WrapWithNullCheck(property, comparison);
    }
}