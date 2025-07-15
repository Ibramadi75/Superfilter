using System.Linq.Expressions;
using Superfilter.Constants;

namespace Superfilter.ExpressionBuilders;

public static class DateExpressionBuilder
{
    public static BinaryExpression BuildDateFilterExpression(Expression property, string filterValue, Operator operatorName)
    {
        if (!DateTime.TryParse(filterValue, out DateTime filterDate))
            throw new FormatException($"Invalid date format: {filterValue}");

        return BuildDateTimeFilterExpression(property, filterDate, operatorName, typeof(DateTime?), DateTime.MinValue);
    }

    public static BinaryExpression BuildDateTimeOffsetFilterExpression(Expression property, string filterValue, Operator operatorName)
    {
        if (!DateTimeOffset.TryParse(filterValue, out DateTimeOffset filterDate))
            throw new FormatException($"Invalid DateTimeOffset format: {filterValue}");

        return BuildDateTimeFilterExpression(property, filterDate, operatorName, typeof(DateTimeOffset?), DateTimeOffset.MinValue);
    }

    private static BinaryExpression BuildDateTimeFilterExpression<T>(Expression property, T filterDate, Operator operatorName, Type nullableType, T minValue)
    {
        Expression propertyValue = property.Type == nullableType
            ? Expression.Coalesce(property, Expression.Constant(minValue))
            : property;

        return operatorName switch
        {
            Operator.Equals => Expression.Equal(propertyValue, Expression.Constant(filterDate)),
            Operator.IsEqualToFullDate => Expression.AndAlso(
                Expression.Equal(Expression.Property(propertyValue, nameof(DateTime.Year)), Expression.Constant(GetYear(filterDate))),
                Expression.AndAlso(
                    Expression.Equal(Expression.Property(propertyValue, nameof(DateTime.Month)), Expression.Constant(GetMonth(filterDate))),
                    Expression.Equal(Expression.Property(propertyValue, nameof(DateTime.Day)), Expression.Constant(GetDay(filterDate)))
                )
            ),
            Operator.IsEqualToYearAndMonth => Expression.AndAlso(
                Expression.Equal(Expression.Property(propertyValue, nameof(DateTime.Year)), Expression.Constant(GetYear(filterDate))),
                Expression.Equal(Expression.Property(propertyValue, nameof(DateTime.Month)), Expression.Constant(GetMonth(filterDate)))
            ),
            Operator.IsEqualToYear => Expression.Equal(Expression.Property(propertyValue, nameof(DateTime.Year)), Expression.Constant(GetYear(filterDate))),

            Operator.LessThan => Expression.LessThan(propertyValue, Expression.Constant(filterDate)),
            Operator.GreaterThan => Expression.GreaterThan(propertyValue, Expression.Constant(filterDate)),

            _ => throw new InvalidOperationException($"Invalid operator for {typeof(T).Name}.")
        };
    }

    private static int GetYear<T>(T date) => date switch
    {
        DateTime dt => dt.Year,
        DateTimeOffset dto => dto.Year,
        _ => throw new InvalidOperationException($"Unsupported date type: {typeof(T).Name}")
    };

    private static int GetMonth<T>(T date) => date switch
    {
        DateTime dt => dt.Month,
        DateTimeOffset dto => dto.Month,
        _ => throw new InvalidOperationException($"Unsupported date type: {typeof(T).Name}")
    };

    private static int GetDay<T>(T date) => date switch
    {
        DateTime dt => dt.Day,
        DateTimeOffset dto => dto.Day,
        _ => throw new InvalidOperationException($"Unsupported date type: {typeof(T).Name}")
    };
}