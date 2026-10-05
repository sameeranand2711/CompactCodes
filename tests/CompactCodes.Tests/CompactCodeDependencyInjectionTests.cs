using CompactCodes;
using Microsoft.Extensions.DependencyInjection;

namespace CompactCodes.Tests;

public sealed class CompactCodeDependencyInjectionTests
{
    [Fact]
    public void AddCompactCodes_RegistersSingletonGenerator()
    {
        var services = new ServiceCollection();
        services.AddCompactCodes();

        using var provider = services.BuildServiceProvider();

        var first = provider.GetRequiredService<ICompactCodeGenerator>();
        var second = provider.GetRequiredService<ICompactCodeGenerator>();

        Assert.Same(first, second);
        Assert.Equal(10, first.Generate().Length);
    }

    [Fact]
    public void AddCompactCodes_AppliesCustomOptions()
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var services = new ServiceCollection();
        services.AddCompactCodes(options =>
        {
            options.Alphabet = alphabet;
            options.MinimumLength = 12;
            options.MaximumLength = 16;
        });

        using var provider = services.BuildServiceProvider();

        var code = provider.GetRequiredService<ICompactCodeGenerator>().Generate();

        Assert.Equal(12, code.Length);
        Assert.All(code, character => Assert.Contains(character, alphabet));
    }

    [Fact]
    public void AddCompactCodes_ValidatesOptionsImmediately()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentException>(() => services.AddCompactCodes(options =>
        {
            options.Alphabet = "0123456789";
            options.MinimumLength = 8;
        }));
    }

    [Fact]
    public void AddCompactCodes_RejectsNullServices()
    {
        Assert.Throws<ArgumentNullException>(() => ServiceCollectionExtensions.AddCompactCodes(null!));
    }

    [Fact]
    public void AddCompactCodes_RepeatedRegistrationKeepsFirstSingleton()
    {
        var services = new ServiceCollection();
        services.AddCompactCodes(options => options.MinimumLength = 12);
        services.AddCompactCodes();

        using var provider = services.BuildServiceProvider();

        var generators = provider.GetServices<ICompactCodeGenerator>().ToArray();

        Assert.Single(generators);
        Assert.Same(generators[0], provider.GetRequiredService<ICompactCodeGenerator>());
        Assert.Equal(12, generators[0].Generate().Length);
    }
}
