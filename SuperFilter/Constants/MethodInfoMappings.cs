using System.Reflection;
using Superfilter.CustomOperations;

namespace Superfilter.Constants;

public static class MethodInfoMappings
{
    private static Dictionary<Operator, MethodInfo?> MethodInfosForStringFiltering => new()
    {
        { Operator.StartsWith, typeof(string).GetMethod("StartsWith", [typeof(string)]) },
        { Operator.Contains, typeof(string).GetMethod("Contains", [typeof(string)]) },
        { Operator.Equals, typeof(string).GetMethod("Equals", [typeof(string)]) }
    };

    private static Dictionary<Operator, MethodInfo?> MethodInfosForDateTimeFiltering => new()
    {
        { Operator.Equals, typeof(DateTime).GetMethod("Equals", [typeof(DateTime)]) },
        { Operator.IsEqualToYear, typeof(DateOperations).GetMethod(nameof(DateOperations.CompareDateByYear), [typeof(DateTime), typeof(DateTime)]) },
        { Operator.IsEqualToYearAndMonth, typeof(DateOperations).GetMethod(nameof(DateOperations.CompareDateByYearAndMonth), [typeof(DateTime), typeof(DateTime)]) },
        { Operator.IsEqualToFullDate, typeof(DateOperations).GetMethod(nameof(DateOperations.CompareDateByYearMonthAndDay), [typeof(DateTime), typeof(DateTime)]) },
        { Operator.LessThan, typeof(DateTime).GetMethod("CompareTo", [typeof(DateTime)]) },
        { Operator.GreaterThan, typeof(DateTime).GetMethod("CompareTo", [typeof(DateTime)]) }
    };

    private static Dictionary<Operator, MethodInfo?> MethodInfosForBooleanFiltering => new()
    {
        { Operator.Equals, typeof(bool).GetMethod("Equals", [typeof(bool)]) }
    };

    private static Dictionary<Operator, MethodInfo?> MethodInfosForIntegerFiltering => new()
    {
        { Operator.Equals, typeof(int).GetMethod("Equals", [typeof(int)]) },
        { Operator.LessThan, typeof(int).GetMethod("LessThan", [typeof(int)]) },
        { Operator.GreaterThan, typeof(int).GetMethod("GreaterThan", [typeof(int)]) }
    };

    private static Dictionary<Operator, MethodInfo?> MethodInfosForLongFiltering => new()
    {
        { Operator.Equals, typeof(long).GetMethod("Equals", [typeof(long)]) },
        { Operator.LessThan, typeof(long).GetMethod("LessThan", [typeof(long)]) },
        { Operator.GreaterThan, typeof(long).GetMethod("GreaterThan", [typeof(long)]) }
    };

    private static Dictionary<Operator, MethodInfo?> MethodInfosForDecimalFiltering => new()
    {
        { Operator.Equals, typeof(decimal).GetMethod("Equals", [typeof(decimal)]) },
        { Operator.LessThan, typeof(decimal).GetMethod("LessThan", [typeof(decimal)]) },
        { Operator.GreaterThan, typeof(decimal).GetMethod("GreaterThan", [typeof(decimal)]) }
    };

    private static Dictionary<Operator, MethodInfo?> MethodInfosForDoubleFiltering => new()
    {
        { Operator.Equals, typeof(double).GetMethod("Equals", [typeof(double)]) },
        { Operator.LessThan, typeof(double).GetMethod("LessThan", [typeof(double)]) },
        { Operator.GreaterThan, typeof(double).GetMethod("GreaterThan", [typeof(double)]) }
    };

    private static Dictionary<Operator, MethodInfo?> MethodInfosForFloatFiltering => new()
    {
        { Operator.Equals, typeof(float).GetMethod("Equals", [typeof(float)]) },
        { Operator.LessThan, typeof(float).GetMethod("LessThan", [typeof(float)]) },
        { Operator.GreaterThan, typeof(float).GetMethod("GreaterThan", [typeof(float)]) }
    };

    public static Dictionary<Operator, MethodInfo?> GetMethodInfos(Type propertyType)
    {
        if (propertyType == typeof(string))
            return MethodInfosForStringFiltering;

        if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
            return MethodInfosForDateTimeFiltering;

        if (propertyType == typeof(bool) || propertyType == typeof(bool?))
            return MethodInfosForBooleanFiltering;

        if (propertyType == typeof(int) || propertyType == typeof(int?))
            return MethodInfosForIntegerFiltering;

        if (propertyType == typeof(long) || propertyType == typeof(long?))
            return MethodInfosForLongFiltering;

        if (propertyType == typeof(decimal) || propertyType == typeof(decimal?))
            return MethodInfosForDecimalFiltering;

        if (propertyType == typeof(double) || propertyType == typeof(double?))
            return MethodInfosForDoubleFiltering;

        if (propertyType == typeof(float) || propertyType == typeof(float?))
            return MethodInfosForFloatFiltering;

        return new Dictionary<Operator, MethodInfo?>();
    }

    public static bool IsOperatorLegal<T>(Operator op)
    {
        Dictionary<Operator, MethodInfo?> methodInfos = GetMethodInfos(typeof(T));
        return methodInfos.Count > 0;
    }
}