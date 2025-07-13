using Superfilter.Entities;

namespace Superfilter.Builder;

/// <summary>
/// Static factory class for creating configuration builders
/// </summary>
public static class SuperfilterBuilder
{
    /// <summary>
    /// Creates a new configuration builder for the specified entity type
    /// </summary>
    /// <typeparam name="T">The entity type to configure</typeparam>
    /// <returns>New ConfigurationBuilder instance</returns>
    public static ConfigurationBuilder<T> For<T>() where T : class
    {
        return new ConfigurationBuilder<T>();
    }

    /// <summary>
    /// Creates a new configuration builder with error strategy
    /// </summary>
    /// <typeparam name="T">The entity type to configure</typeparam>
    /// <param name="errorStrategy">Error handling strategy</param>
    /// <returns>New ConfigurationBuilder instance</returns>
    public static ConfigurationBuilder<T> For<T>(OnErrorStrategy errorStrategy) where T : class
    {
        return new ConfigurationBuilder<T>().WithErrorStrategy(errorStrategy);
    }
}