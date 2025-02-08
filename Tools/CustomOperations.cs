namespace SuperFilter
{
    public static class CustomOperations
    {
        private enum Precision
        {
            YearOnly,
            YearAndMonth,
            YearAndMonthAndDay,
            Everything
        }

        public static bool CompareDate(DateTime? dt1, DateTime? dt2)
        {
            return CompareDateWithPrecision(dt1, dt2, Precision.Everything);
        }

        public static bool CompareDateByYear(DateTime? dt1, DateTime? dt2)
        {
            return CompareDateWithPrecision(dt1, dt2, Precision.YearOnly);
        }

        public static bool CompareDateByYearAndMonth(DateTime? dt1, DateTime? dt2)
        {
            return CompareDateWithPrecision(dt1, dt2, Precision.YearAndMonth);
        }

        public static bool CompareDateByYearMonthAndDay(DateTime? dt1, DateTime? dt2)
        {
            return CompareDateWithPrecision(dt1, dt2, Precision.YearAndMonthAndDay);
        }

        private static bool CompareDateWithPrecision(DateTime? dt1, DateTime? dt2, Precision precision = Precision.Everything)
        {
            if (!dt1.HasValue && !dt2.HasValue)
                return true;
            if (!dt1.HasValue || !dt2.HasValue)
                return false;

            return precision switch
            {
                Precision.YearOnly => dt1.Value.Year == dt2.Value.Year,
                Precision.YearAndMonth => dt1.Value.Year == dt2.Value.Year && dt1.Value.Month == dt2.Value.Month,
                Precision.YearAndMonthAndDay => dt1.Value.Date == dt2.Value.Date,
                _ => throw new ArgumentOutOfRangeException(nameof(precision), precision, "Invalid precision type")
            };
        }
    }
}