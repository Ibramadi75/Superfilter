using System.Linq.Expressions;
using Superfilter.Constants;

namespace Superfilter.ExpressionBuilders.Common;

public static class CommonExpressionBuilder
{
    private static BinaryExpression BuildNullCheckExpression(Expression property, Operator filterOperator)
    {
        return filterOperator switch
        {
            Operator.IsNull => Expression.Equal(property, Expression.Constant(null, property.Type)),
            Operator.IsNotNull => Expression.NotEqual(property, Expression.Constant(null, property.Type)),
            _ => throw new InvalidOperationException($"Invalid null check operator: {filterOperator}")
        };
    }

    public static Expression BuildComparisonExpression(Expression property, Expression constant, Operator filterOperator)
    {
        return filterOperator switch
        {
            Operator.Equals => Expression.Equal(property, constant),
            Operator.NotEquals => Expression.NotEqual(property, constant),
            Operator.LessThan => Expression.LessThan(property, constant),
            Operator.LessThanOrEqual => Expression.LessThanOrEqual(property, constant),
            Operator.GreaterThan => Expression.GreaterThan(property, constant),
            Operator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(property, constant),
            _ => throw new InvalidOperationException($"Invalid comparison operator: {filterOperator}")
        };
    }

    private static Expression BuildInExpression(Expression[] comparisons)
        => comparisons.Length == 0 ? Expression.Constant(false) : comparisons.Aggregate(Expression.OrElse);

    private static BinaryExpression BuildBetweenExpression(Expression property, Expression minConstant, Expression maxConstant)
    {
        return Expression.AndAlso(
            Expression.GreaterThanOrEqual(property, minConstant),
            Expression.LessThanOrEqual(property, maxConstant)
        );
    }

    public static Expression BuildInExpressionWithParser<T>(Expression property, string filterValue, Func<string, T> parser, string typeName)
    {
        string[] values = filterValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
        
        if (values.Length == 0)
            return Expression.Constant(false);

        Expression[] comparisons = values.Select(value =>
        {
            try
            {
                T parsedValue = parser(value.Trim());
                return Expression.Equal(property, Expression.Convert(Expression.Constant(parsedValue), property.Type));
            }
            catch
            {
                throw new FormatException($"Invalid {typeName} format: {value}");
            }
        }).ToArray<Expression>();

        return BuildInExpression(comparisons);
    }

    public static Expression BuildBetweenExpressionWithParser<T>(Expression property, string filterValue, Func<string, T> parser, string typeName)
    {
        string[] values = filterValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
        
        if (values.Length != 2)
            throw new ArgumentException("Between operator requires exactly two values separated by comma");

        try
        {
            T minValue = parser(values[0].Trim());
            T maxValue = parser(values[1].Trim());

            Expression minConstant = Expression.Convert(Expression.Constant(minValue), property.Type);
            Expression maxConstant = Expression.Convert(Expression.Constant(maxValue), property.Type);

            return BuildBetweenExpression(property, minConstant, maxConstant);
        }
        catch
        {
            throw new FormatException($"Invalid {typeName} format in between values");
        }
    }

    public static Expression BuildComplexFilterExpression(Expression property, string filterValue, Operator filterOperator, 
        Func<Expression, string, Expression> inBuilder, Func<Expression, string, Expression> betweenBuilder,
        Func<Expression, string, Operator, Expression> comparisonBuilder)
    {
        return filterOperator switch
        {
            Operator.IsNull or Operator.IsNotNull => BuildNullCheckExpression(property, filterOperator),
            Operator.In => inBuilder(property, filterValue),
            Operator.NotIn => Expression.Not(inBuilder(property, filterValue)),
            Operator.Between => betweenBuilder(property, filterValue),
            Operator.NotBetween => Expression.Not(betweenBuilder(property, filterValue)),
            _ => comparisonBuilder(property, filterValue, filterOperator)
        };
    }

    public static Expression WrapWithNullCheck(Expression property, Expression expression)
    {
        if (IsNullableType(property.Type))
        {
            return Expression.AndAlso(
                Expression.NotEqual(property, Expression.Constant(null, property.Type)),
                expression
            );
        }
        return expression;
    }

    private static bool IsNullableType(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }
}