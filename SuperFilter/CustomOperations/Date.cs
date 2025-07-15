using System.Linq.Expressions;

namespace Superfilter.CustomOperations;

public static class DateOperations
{
    public static Expression CompareDateByYear(Expression property, Expression filterDate)
    {
        MemberExpression yearProperty = Expression.Property(property, nameof(DateTime.Year));
        MemberExpression yearConstant = Expression.Property(filterDate, nameof(DateTime.Year));
        return Expression.Equal(yearProperty, yearConstant);
    }

    public static Expression CompareDateByYearAndMonth(Expression property, Expression filterDate)
    {
        MemberExpression yearProperty = Expression.Property(property, nameof(DateTime.Year));
        MemberExpression yearConstant = Expression.Property(filterDate, nameof(DateTime.Year));
        MemberExpression monthProperty = Expression.Property(property, nameof(DateTime.Month));
        MemberExpression monthConstant = Expression.Property(filterDate, nameof(DateTime.Month));

        BinaryExpression yearComparison = Expression.Equal(yearProperty, yearConstant);
        BinaryExpression monthComparison = Expression.Equal(monthProperty, monthConstant);

        return Expression.AndAlso(yearComparison, monthComparison);
    }

    public static Expression CompareDateByYearMonthAndDay(Expression property, Expression filterDate)
    {
        MemberExpression yearProperty = Expression.Property(property, nameof(DateTime.Year));
        MemberExpression yearConstant = Expression.Property(filterDate, nameof(DateTime.Year));
        MemberExpression monthProperty = Expression.Property(property, nameof(DateTime.Month));
        MemberExpression monthConstant = Expression.Property(filterDate, nameof(DateTime.Month));
        MemberExpression dayProperty = Expression.Property(property, nameof(DateTime.Day));
        MemberExpression dayConstant = Expression.Property(filterDate, nameof(DateTime.Day));

        BinaryExpression yearComparison = Expression.Equal(yearProperty, yearConstant);
        BinaryExpression monthComparison = Expression.Equal(monthProperty, monthConstant);
        BinaryExpression dayComparison = Expression.Equal(dayProperty, dayConstant);

        return Expression.AndAlso(Expression.AndAlso(yearComparison, monthComparison), dayComparison);
    }

    public static Expression CompareDateTimeOffsetByYear(Expression property, Expression filterDate)
    {
        MemberExpression yearProperty = Expression.Property(property, nameof(DateTimeOffset.Year));
        MemberExpression yearConstant = Expression.Property(filterDate, nameof(DateTimeOffset.Year));
        return Expression.Equal(yearProperty, yearConstant);
    }

    public static Expression CompareDateTimeOffsetByYearAndMonth(Expression property, Expression filterDate)
    {
        MemberExpression yearProperty = Expression.Property(property, nameof(DateTimeOffset.Year));
        MemberExpression yearConstant = Expression.Property(filterDate, nameof(DateTimeOffset.Year));
        MemberExpression monthProperty = Expression.Property(property, nameof(DateTimeOffset.Month));
        MemberExpression monthConstant = Expression.Property(filterDate, nameof(DateTimeOffset.Month));

        BinaryExpression yearComparison = Expression.Equal(yearProperty, yearConstant);
        BinaryExpression monthComparison = Expression.Equal(monthProperty, monthConstant);

        return Expression.AndAlso(yearComparison, monthComparison);
    }

    public static Expression CompareDateTimeOffsetByYearMonthAndDay(Expression property, Expression filterDate)
    {
        MemberExpression yearProperty = Expression.Property(property, nameof(DateTimeOffset.Year));
        MemberExpression yearConstant = Expression.Property(filterDate, nameof(DateTimeOffset.Year));
        MemberExpression monthProperty = Expression.Property(property, nameof(DateTimeOffset.Month));
        MemberExpression monthConstant = Expression.Property(filterDate, nameof(DateTimeOffset.Month));
        MemberExpression dayProperty = Expression.Property(property, nameof(DateTimeOffset.Day));
        MemberExpression dayConstant = Expression.Property(filterDate, nameof(DateTimeOffset.Day));

        BinaryExpression yearComparison = Expression.Equal(yearProperty, yearConstant);
        BinaryExpression monthComparison = Expression.Equal(monthProperty, monthConstant);
        BinaryExpression dayComparison = Expression.Equal(dayProperty, dayConstant);

        return Expression.AndAlso(Expression.AndAlso(yearComparison, monthComparison), dayComparison);
    }
}