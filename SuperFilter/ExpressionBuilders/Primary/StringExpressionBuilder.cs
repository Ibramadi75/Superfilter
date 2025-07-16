using System.Linq.Expressions;
using System.Reflection;
using Superfilter.Constants;

namespace Superfilter.ExpressionBuilders;

public static class StringExpressionBuilder
{
    public static Expression? BuildStringFilterExpression(Expression property, string filterValue, Operator op)
    {
        return op switch
        {
            Operator.Equals => Expression.Equal(property, Expression.Constant(filterValue)),
            Operator.NotEquals => Expression.NotEqual(property, Expression.Constant(filterValue)),
            Operator.StartsWith => Expression.Call(property, typeof(string).GetMethod("StartsWith", [typeof(string)])!, Expression.Constant(filterValue)),
            Operator.EndsWith => Expression.Call(property, typeof(string).GetMethod("EndsWith", [typeof(string)])!, Expression.Constant(filterValue)),
            Operator.Contains => Expression.Call(property, typeof(string).GetMethod("Contains", [typeof(string)])!, Expression.Constant(filterValue)),
            Operator.NotContains => Expression.Not(Expression.Call(property, typeof(string).GetMethod("Contains", [typeof(string)])!, Expression.Constant(filterValue))),
            Operator.IsNull => Expression.Equal(property, Expression.Constant(null, typeof(string))),
            Operator.IsNotNull => Expression.NotEqual(property, Expression.Constant(null, typeof(string))),
            Operator.IsEmpty => Expression.Equal(property, Expression.Constant(string.Empty)),
            Operator.IsNotEmpty => Expression.NotEqual(property, Expression.Constant(string.Empty)),
            Operator.In => BuildInExpression(property, filterValue),
            Operator.NotIn => Expression.Not(BuildInExpression(property, filterValue)),
            _ => throw new InvalidOperationException($"Unsupported operator {op} for string type")
        };
    }

    private static Expression BuildInExpression(Expression property, string filterValue)
    {
        string[] values = filterValue.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(v => v.Trim())
            .ToArray();
        
        if (values.Length == 0)
            return Expression.Constant(false);
        
        Expression[] comparisons = values.Select(value => 
            Expression.Equal(property, Expression.Constant(value))).ToArray();
        
        return comparisons.Aggregate(Expression.OrElse);
    }
}