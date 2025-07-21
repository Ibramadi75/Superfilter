using System.Linq.Expressions;

namespace Superfilter.Entities;

public class FieldConfiguration(LambdaExpression selector, bool isRequired = false)
{
    public bool IsRequired { get; set; } = isRequired;
    public LambdaExpression Selector { get; set; } = selector;

    public string GetPropertyName()
    {
        Expression body = Selector.Body is UnaryExpression unary ? unary.Operand : Selector.Body;
        if (body is MemberExpression member)
            return member.Member.Name;
        throw new InvalidOperationException("Cannot extract property name from selector expression");
    }
}