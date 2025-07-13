using System.Linq.Expressions;
using Superfilter.Constants;

namespace Superfilter.ExpressionBuilders;

public static class DateExpressionBuilder
{
    public static BinaryExpression BuildDateFilterExpression(Expression property, string filterValue, Operator operatorName)
    {
        if (!DateTime.TryParse(filterValue, out DateTime filterDate))
            throw new FormatException($"Invalid date format: {filterValue}");

        Expression propertyValue = property.Type == typeof(DateTime?)
            ? Expression.Coalesce(property, Expression.Constant(DateTime.MinValue))
            : property;

        return operatorName switch
        {
            Operator.Equals => Expression.Equal(propertyValue, Expression.Constant(filterDate)),
            Operator.IsEqualToFullDate => Expression.AndAlso(
                Expression.Equal(Expression.Property(propertyValue, nameof(DateTime.Year)), Expression.Constant(filterDate.Year)),
                Expression.AndAlso(
                    Expression.Equal(Expression.Property(propertyValue, nameof(DateTime.Month)), Expression.Constant(filterDate.Month)),
                    Expression.Equal(Expression.Property(propertyValue, nameof(DateTime.Day)), Expression.Constant(filterDate.Day))
                )
            ),
            Operator.IsEqualToYearAndMonth => Expression.AndAlso(
                Expression.Equal(Expression.Property(propertyValue, nameof(DateTime.Year)), Expression.Constant(filterDate.Year)),
                Expression.Equal(Expression.Property(propertyValue, nameof(DateTime.Month)), Expression.Constant(filterDate.Month))
            ),
            Operator.IsEqualToYear => Expression.Equal(Expression.Property(propertyValue, nameof(DateTime.Year)), Expression.Constant(filterDate.Year)),

            Operator.LessThan => Expression.LessThan(propertyValue, Expression.Constant(filterDate)),
            Operator.GreaterThan => Expression.GreaterThan(propertyValue, Expression.Constant(filterDate)),

            _ => throw new InvalidOperationException("Invalid operator for date.")
        };
    }
}