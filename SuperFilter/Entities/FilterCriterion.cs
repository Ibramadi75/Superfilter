using SuperFilter.Constants;

namespace SuperFilter.Entities;

public record FilterCriterion(string Field, Operator Operator, string Value);