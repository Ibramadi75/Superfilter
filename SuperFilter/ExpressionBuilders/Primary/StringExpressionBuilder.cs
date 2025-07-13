using System.Linq.Expressions;
using System.Reflection;
using Superfilter.Constants;

namespace Superfilter.ExpressionBuilders;

public static class StringExpressionBuilder
{
    public static MethodCallExpression? BuildStringFilterExpression(Expression property, string filterValue, Operator op)
    {
        MethodInfo? method = MethodInfoMappings.GetMethodInfos(typeof(string))[op];
        return method is null ? null : Expression.Call(property, method, Expression.Constant(filterValue));
    }
}