using System.Reflection;
using Superfilter.CustomOperations;

namespace Superfilter.Constants;

public static class MethodInfoMappings
{
    private static Dictionary<Operator, MethodInfo?> MethodInfosForStringFiltering => new()
    {
        { Operator.StartsWith, typeof(string).GetMethod("StartsWith", [typeof(string)]) },
        { Operator.EndsWith, typeof(string).GetMethod("EndsWith", [typeof(string)]) },
        { Operator.Contains, typeof(string).GetMethod("Contains", [typeof(string)]) },
        { Operator.Equals, typeof(string).GetMethod("Equals", [typeof(string)]) },
        { Operator.NotEquals, typeof(string).GetMethod("Equals", [typeof(string)]) },
        { Operator.NotContains, typeof(string).GetMethod("Contains", [typeof(string)]) },
        { Operator.IsNull, null },
        { Operator.IsNotNull, null },
        { Operator.IsEmpty, null },
        { Operator.IsNotEmpty, null },
        { Operator.In, null },
        { Operator.NotIn, null }
    };

    private static Dictionary<Operator, MethodInfo?> MethodInfosForDateTimeFiltering => new()
    {
        { Operator.Equals, typeof(DateTime).GetMethod("Equals", [typeof(DateTime)]) },
        { Operator.NotEquals, typeof(DateTime).GetMethod("Equals", [typeof(DateTime)]) },
        { Operator.IsEqualToYear, typeof(DateOperations).GetMethod(nameof(DateOperations.CompareDateByYear), [typeof(DateTime), typeof(DateTime)]) },
        { Operator.IsEqualToYearAndMonth, typeof(DateOperations).GetMethod(nameof(DateOperations.CompareDateByYearAndMonth), [typeof(DateTime), typeof(DateTime)]) },
        { Operator.IsEqualToFullDate, typeof(DateOperations).GetMethod(nameof(DateOperations.CompareDateByYearMonthAndDay), [typeof(DateTime), typeof(DateTime)]) },
        { Operator.LessThan, typeof(DateTime).GetMethod("CompareTo", [typeof(DateTime)]) },
        { Operator.LessThanOrEqual, typeof(DateTime).GetMethod("CompareTo", [typeof(DateTime)]) },
        { Operator.GreaterThan, typeof(DateTime).GetMethod("CompareTo", [typeof(DateTime)]) },
        { Operator.GreaterThanOrEqual, typeof(DateTime).GetMethod("CompareTo", [typeof(DateTime)]) },
        { Operator.IsBefore, typeof(DateTime).GetMethod("CompareTo", [typeof(DateTime)]) },
        { Operator.IsAfter, typeof(DateTime).GetMethod("CompareTo", [typeof(DateTime)]) },
        { Operator.IsBetween, null },
        { Operator.Between, null },
        { Operator.NotBetween, null },
        { Operator.IsNull, null },
        { Operator.IsNotNull, null },
        { Operator.In, null },
        { Operator.NotIn, null }
    };

    private static Dictionary<Operator, MethodInfo?> MethodInfosForBooleanFiltering => new()
    {
        { Operator.Equals, typeof(bool).GetMethod("Equals", [typeof(bool)]) },
        { Operator.NotEquals, typeof(bool).GetMethod("Equals", [typeof(bool)]) },
        { Operator.IsNull, null },
        { Operator.IsNotNull, null }
    };

    private static Dictionary<Operator, MethodInfo?> MethodInfosForIntegerFiltering => new()
    {
        { Operator.Equals, typeof(int).GetMethod("Equals", [typeof(int)]) },
        { Operator.NotEquals, typeof(int).GetMethod("Equals", [typeof(int)]) },
        { Operator.LessThan, typeof(int).GetMethod("LessThan", [typeof(int)]) },
        { Operator.LessThanOrEqual, typeof(int).GetMethod("LessThan", [typeof(int)]) },
        { Operator.GreaterThan, typeof(int).GetMethod("GreaterThan", [typeof(int)]) },
        { Operator.GreaterThanOrEqual, typeof(int).GetMethod("GreaterThan", [typeof(int)]) },
        { Operator.Between, null },
        { Operator.NotBetween, null },
        { Operator.In, null },
        { Operator.NotIn, null },
        { Operator.IsNull, null },
        { Operator.IsNotNull, null }
    };

    private static Dictionary<Operator, MethodInfo?> MethodInfosForLongFiltering => new()
    {
        { Operator.Equals, typeof(long).GetMethod("Equals", [typeof(long)]) },
        { Operator.NotEquals, typeof(long).GetMethod("Equals", [typeof(long)]) },
        { Operator.LessThan, typeof(long).GetMethod("LessThan", [typeof(long)]) },
        { Operator.LessThanOrEqual, typeof(long).GetMethod("LessThan", [typeof(long)]) },
        { Operator.GreaterThan, typeof(long).GetMethod("GreaterThan", [typeof(long)]) },
        { Operator.GreaterThanOrEqual, typeof(long).GetMethod("GreaterThan", [typeof(long)]) },
        { Operator.Between, null },
        { Operator.NotBetween, null },
        { Operator.In, null },
        { Operator.NotIn, null },
        { Operator.IsNull, null },
        { Operator.IsNotNull, null }
    };

    private static Dictionary<Operator, MethodInfo?> MethodInfosForDecimalFiltering => new()
    {
        { Operator.Equals, typeof(decimal).GetMethod("Equals", [typeof(decimal)]) },
        { Operator.NotEquals, typeof(decimal).GetMethod("Equals", [typeof(decimal)]) },
        { Operator.LessThan, typeof(decimal).GetMethod("LessThan", [typeof(decimal)]) },
        { Operator.LessThanOrEqual, typeof(decimal).GetMethod("LessThan", [typeof(decimal)]) },
        { Operator.GreaterThan, typeof(decimal).GetMethod("GreaterThan", [typeof(decimal)]) },
        { Operator.GreaterThanOrEqual, typeof(decimal).GetMethod("GreaterThan", [typeof(decimal)]) },
        { Operator.Between, null },
        { Operator.NotBetween, null },
        { Operator.In, null },
        { Operator.NotIn, null },
        { Operator.IsNull, null },
        { Operator.IsNotNull, null }
    };

    private static Dictionary<Operator, MethodInfo?> MethodInfosForDoubleFiltering => new()
    {
        { Operator.Equals, typeof(double).GetMethod("Equals", [typeof(double)]) },
        { Operator.NotEquals, typeof(double).GetMethod("Equals", [typeof(double)]) },
        { Operator.LessThan, typeof(double).GetMethod("LessThan", [typeof(double)]) },
        { Operator.LessThanOrEqual, typeof(double).GetMethod("LessThan", [typeof(double)]) },
        { Operator.GreaterThan, typeof(double).GetMethod("GreaterThan", [typeof(double)]) },
        { Operator.GreaterThanOrEqual, typeof(double).GetMethod("GreaterThan", [typeof(double)]) },
        { Operator.Between, null },
        { Operator.NotBetween, null },
        { Operator.In, null },
        { Operator.NotIn, null },
        { Operator.IsNull, null },
        { Operator.IsNotNull, null }
    };

    private static Dictionary<Operator, MethodInfo?> MethodInfosForFloatFiltering => new()
    {
        { Operator.Equals, typeof(float).GetMethod("Equals", [typeof(float)]) },
        { Operator.NotEquals, typeof(float).GetMethod("Equals", [typeof(float)]) },
        { Operator.LessThan, typeof(float).GetMethod("LessThan", [typeof(float)]) },
        { Operator.LessThanOrEqual, typeof(float).GetMethod("LessThan", [typeof(float)]) },
        { Operator.GreaterThan, typeof(float).GetMethod("GreaterThan", [typeof(float)]) },
        { Operator.GreaterThanOrEqual, typeof(float).GetMethod("GreaterThan", [typeof(float)]) },
        { Operator.Between, null },
        { Operator.NotBetween, null },
        { Operator.In, null },
        { Operator.NotIn, null },
        { Operator.IsNull, null },
        { Operator.IsNotNull, null }
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
        return methodInfos.ContainsKey(op);
    }
}