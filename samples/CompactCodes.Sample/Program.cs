using CompactCodes;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddCompactCodes();

var customServices = new ServiceCollection();
customServices.AddCompactCodes(options =>
{
    options.Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    options.MinimumLength = 12;
    options.MaximumLength = 16;
});

using var provider = services.BuildServiceProvider();
using var customProvider = customServices.BuildServiceProvider();

Console.WriteLine($"Default code: {provider.GetRequiredService<ICompactCodeGenerator>().Generate()}");
Console.WriteLine($"Custom-alphabet code: {customProvider.GetRequiredService<ICompactCodeGenerator>().Generate()}");
Console.WriteLine("For uniqueness, enforce a database UNIQUE constraint and retry on collision.");
