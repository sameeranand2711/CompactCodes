using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CompactCodes;

public static class ServiceCollectionExtensions
{
    /// <summary>Registers a validated, singleton compact-code generator.</summary>
    /// <remarks>
    /// Repeated calls do not replace an existing generator registration; the first registration wins.
    /// </remarks>
    public static IServiceCollection AddCompactCodes(
        this IServiceCollection services,
        Action<CompactCodeOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new CompactCodeOptions();
        configureOptions?.Invoke(options);

        var generator = new DistributedRandomCodeGenerator(options);
        services.TryAddSingleton<ICompactCodeGenerator>(generator);
        return services;
    }
}
