using System.Diagnostics.CodeAnalysis;
using Superfilter.Entities;

namespace Superfilter;

public partial class Superfilter
{
    private void ValidateRequiredFiltersPresence()
    {
        EnsureGlobalConfigurationIsSet();
        
        foreach ((string? fieldConfigKey, FieldConfiguration? fieldConfig) in GlobalConfiguration.PropertyMappings)
        {
            if (IsRequiredFilterPresent(fieldConfig, fieldConfigKey)) continue;
            throw new SuperfilterException($"Filter {fieldConfigKey} is required.");
        }
    }
    
    private bool IsRequiredFilterPresent(FieldConfiguration fieldConfig, string propertyName)
    {
        EnsureGlobalConfigurationIsSet();
        
        bool filterIsPresent = GlobalConfiguration.HasFilters != null && GlobalConfiguration.HasFilters.Filters
            .Any(filters => string.Equals(filters.Field, propertyName, StringComparison.CurrentCultureIgnoreCase));
        
        return !fieldConfig.IsRequired || filterIsPresent;
    }

    [MemberNotNull(nameof(GlobalConfiguration))]
    private void EnsureGlobalConfigurationIsSet()
    {
        if (GlobalConfiguration == null)
            throw new SuperfilterException("GlobalConfiguration must be set before using AddFieldConfiguration");
    }
    
    private void EnsureHasFiltersIsSet()
    {
        if (GlobalConfiguration?.HasFilters == null)
            throw new SuperfilterException("HasFilters must be set before using AddFieldConfiguration");
    }
}