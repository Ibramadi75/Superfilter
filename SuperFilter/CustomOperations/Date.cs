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
}