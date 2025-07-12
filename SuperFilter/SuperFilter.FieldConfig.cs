using Superfilter.Entities;

namespace Superfilter;

public partial class Superfilter
{
    private Dictionary<string, FieldConfiguration>? GetFieldConfigurationsForType<T>()
    {
        if (FieldConfigurations.ContainsKey(typeof(T)))
            return FieldConfigurations[typeof(T)] as Dictionary<string, FieldConfiguration>;

        return new Dictionary<string, FieldConfiguration>();
    }
}