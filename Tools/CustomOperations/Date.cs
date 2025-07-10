using System.Linq.Expressions;

namespace SuperFilter.CustomOperations;
public static class DateOperations
{
    public static Expression CompareDateByYear(Expression property, Expression filterDate)
    {
        var yearProperty = Expression.Property(property, nameof(DateTime.Year));
        var yearConstant = Expression.Property(filterDate, nameof(DateTime.Year));
        return Expression.Equal(yearProperty, yearConstant);
    }

    public static Expression CompareDateByYearAndMonth(Expression property, Expression filterDate)
    {
        var yearProperty = Expression.Property(property, nameof(DateTime.Year));
        var yearConstant = Expression.Property(filterDate, nameof(DateTime.Year));
        var monthProperty = Expression.Property(property, nameof(DateTime.Month));
        var monthConstant = Expression.Property(filterDate, nameof(DateTime.Month));

        var yearComparison = Expression.Equal(yearProperty, yearConstant);
        var monthComparison = Expression.Equal(monthProperty, monthConstant);

        return Expression.AndAlso(yearComparison, monthComparison);
    }

    public static Expression CompareDateByYearMonthAndDay(Expression property, Expression filterDate)
    {
        var yearProperty = Expression.Property(property, nameof(DateTime.Year));
        var yearConstant = Expression.Property(filterDate, nameof(DateTime.Year));
        var monthProperty = Expression.Property(property, nameof(DateTime.Month));
        var monthConstant = Expression.Property(filterDate, nameof(DateTime.Month));
        var dayProperty = Expression.Property(property, nameof(DateTime.Day));
        var dayConstant = Expression.Property(filterDate, nameof(DateTime.Day));

        var yearComparison = Expression.Equal(yearProperty, yearConstant);
        var monthComparison = Expression.Equal(monthProperty, monthConstant);
        var dayComparison = Expression.Equal(dayProperty, dayConstant);

        return Expression.AndAlso(Expression.AndAlso(yearComparison, monthComparison), dayComparison);
    }
}
