using System.Linq.Expressions;
using Superfilter.Constants;

namespace Superfilter.ExpressionBuilders;

public static class DateExpressionBuilder
{
    public static Expression BuildDateFilterExpression(Expression property, string filterValue, Operator operatorName)
    {
        // Handle null check operators first (they don't need date parsing)
        if (operatorName == Operator.IsNull)
            return Expression.Equal(property, Expression.Constant(null, property.Type));
        
        if (operatorName == Operator.IsNotNull)
            return Expression.NotEqual(property, Expression.Constant(null, property.Type));

        if (!DateTime.TryParse(filterValue, out DateTime filterDate))
            throw new FormatException($"Invalid date format: {filterValue}");

        return BuildDateTimeFilterExpression(property, filterDate, operatorName, typeof(DateTime?));
    }

    public static Expression BuildDateTimeOffsetFilterExpression(Expression property, string filterValue, Operator operatorName)
    {
        // Handle null check operators first (they don't need date parsing)
        if (operatorName == Operator.IsNull)
            return Expression.Equal(property, Expression.Constant(null, property.Type));
        
        if (operatorName == Operator.IsNotNull)
            return Expression.NotEqual(property, Expression.Constant(null, property.Type));

        if (!DateTimeOffset.TryParse(filterValue, out DateTimeOffset filterDate))
            throw new FormatException($"Invalid DateTimeOffset format: {filterValue}");

        return BuildDateTimeFilterExpression(property, filterDate, operatorName, typeof(DateTimeOffset?));
    }

    private static Expression BuildDateTimeFilterExpression<T>(Expression property, T filterDate, Operator operatorName, Type nullableType)
    {
        // For nullable types, we need to handle null values differently based on the operator
        if (property.Type != nullableType)
            return operatorName switch
            {
                Operator.Equals => Expression.Equal(property, Expression.Constant(filterDate)),
                Operator.IsEqualToFullDate => CreateDatePartComparison(property, filterDate, true, true, true),
                Operator.IsEqualToYearAndMonth => CreateDatePartComparison(property, filterDate, true, true, false),
                Operator.IsEqualToYear => CreateDatePartComparison(property, filterDate, true, false, false),
                Operator.LessThan => Expression.LessThan(property, Expression.Constant(filterDate)),
                Operator.LessThanOrEqual => Expression.LessThanOrEqual(property, Expression.Constant(filterDate)),
                Operator.GreaterThan => Expression.GreaterThan(property, Expression.Constant(filterDate)),
                Operator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(property, Expression.Constant(filterDate)),
                Operator.NotEquals => Expression.NotEqual(property, Expression.Constant(filterDate)),
                Operator.IsBefore => Expression.LessThan(property, Expression.Constant(filterDate)),
                Operator.IsAfter => Expression.GreaterThan(property, Expression.Constant(filterDate)),
                _ => throw new InvalidOperationException($"Invalid operator for {typeof(T).Name}.")
            };
        // For comparison operators, exclude null values
        Expression comparison = operatorName switch
        {
            Operator.Equals => Expression.Equal(property, Expression.Constant(filterDate, property.Type)),
            Operator.IsEqualToFullDate => CreateDatePartComparison(property, filterDate, true, true, true),
            Operator.IsEqualToYearAndMonth => CreateDatePartComparison(property, filterDate, true, true, false),
            Operator.IsEqualToYear => CreateDatePartComparison(property, filterDate, true, false, false),
            Operator.LessThan => Expression.LessThan(property, Expression.Constant(filterDate, property.Type)),
            Operator.LessThanOrEqual => Expression.LessThanOrEqual(property, Expression.Constant(filterDate, property.Type)),
            Operator.GreaterThan => Expression.GreaterThan(property, Expression.Constant(filterDate, property.Type)),
            Operator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(property, Expression.Constant(filterDate, property.Type)),
            Operator.NotEquals => Expression.NotEqual(property, Expression.Constant(filterDate, property.Type)),
            Operator.IsBefore => Expression.LessThan(property, Expression.Constant(filterDate, property.Type)),
            Operator.IsAfter => Expression.GreaterThan(property, Expression.Constant(filterDate, property.Type)),
            _ => throw new InvalidOperationException($"Invalid operator for {typeof(T).Name}.")
        };
            
        return Expression.AndAlso(Expression.NotEqual(property, Expression.Constant(null, property.Type)), comparison);
    }

    private static Expression CreateDatePartComparison<T>(Expression property, T filterDate, bool compareYear, bool compareMonth, bool compareDay)
    {
        Expression comparison = Expression.Constant(true);
        
        Expression actualProperty = property.Type.IsGenericType && property.Type.GetGenericTypeDefinition() == typeof(Nullable<>)
            ? Expression.Property(property, "Value")
            : property;
        
        if (compareYear)
        {
            Expression yearComparison = Expression.Equal(
                Expression.Property(actualProperty, nameof(DateTime.Year)),
                Expression.Constant(GetYear(filterDate))
            );
            comparison = Expression.AndAlso(comparison, yearComparison);
        }
        
        if (compareMonth)
        {
            Expression monthComparison = Expression.Equal(
                Expression.Property(actualProperty, nameof(DateTime.Month)),
                Expression.Constant(GetMonth(filterDate))
            );
            comparison = Expression.AndAlso(comparison, monthComparison);
        }
        
        if (compareDay)
        {
            Expression dayComparison = Expression.Equal(
                Expression.Property(actualProperty, nameof(DateTime.Day)),
                Expression.Constant(GetDay(filterDate))
            );
            comparison = Expression.AndAlso(comparison, dayComparison);
        }
        
        return comparison;
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