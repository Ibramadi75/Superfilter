using System.Linq.Expressions;

namespace SuperFilter.Entities;

public class FieldConfiguration
{
    public string EntityPropertyName { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = false;
    public LambdaExpression Selector { get; set; }
}