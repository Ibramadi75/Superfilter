using System.Diagnostics.CodeAnalysis;
using SuperFilter.Entities;

namespace SuperFilter;

public partial class Superfilter
{
    private void ValidateRequiredFiltersPresence()
    {
        EnsureGlobalConfigurationIsSet();
        
        foreach (KeyValuePair<string, FieldConfiguration> propertyMapping in GlobalConfiguration.PropertyMappings)
        {
            FieldConfiguration fieldConfig = propertyMapping.Value;
            if (IsRequiredFilterPresent(fieldConfig, propertyMapping.Key)) continue;
            throw new SuperFilterException($"Filter {propertyMapping.Key} is required.");
        }
    }
    
    private bool IsRequiredFilterPresent(FieldConfiguration fieldConfig, string propertyName)
    {
        EnsureGlobalConfigurationIsSet();
        
        bool filterIsPresent = GlobalConfiguration.HasFilters.Filters
            .Any(filters => string.Equals(filters.Field, propertyName, StringComparison.CurrentCultureIgnoreCase));
        
        return !fieldConfig.IsRequired || filterIsPresent;
    }

    [MemberNotNull(nameof(GlobalConfiguration))]
    private void EnsureGlobalConfigurationIsSet()
    {
        if (GlobalConfiguration == null)
            throw new SuperFilterException("GlobalConfiguration must be set before using AddFieldConfiguration");
    }
}