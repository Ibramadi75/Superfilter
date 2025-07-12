using System.Diagnostics.CodeAnalysis;
using SuperFilter.Entities;

namespace SuperFilter;

public partial class Superfilter
{
    private void ValidateRequiredFiltersPresence()
    {
        EnsureGlobalConfigurationIsSet();
        
        foreach ((string? fieldConfigKey, FieldConfiguration? fieldConfig) in GlobalConfiguration.PropertyMappings)
        {
            if (IsRequiredFilterPresent(fieldConfig, fieldConfigKey)) continue;
            throw new SuperFilterException($"Filter {fieldConfigKey} is required.");
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