using Superfilter.Constants;

namespace Superfilter.Entities;

public record FilterCriterion(string Field, Operator Operator, string Value);