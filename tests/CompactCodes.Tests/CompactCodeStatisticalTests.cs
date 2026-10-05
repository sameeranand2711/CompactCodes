using CompactCodes;
using Xunit.Abstractions;

namespace CompactCodes.Tests;

public sealed class CompactCodeStatisticalTests
{
    private readonly ITestOutputHelper _output;

    public CompactCodeStatisticalTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Generate_HasBroadlyUniformCharacterAndPositionFrequencies()
    {
        const int sampleCount = 20_000;
        var alphabet = CompactCodeOptions.Base62Alphabet;
        var generator = new DistributedRandomCodeGenerator();
        var characterCounts = alphabet.ToDictionary(character => character, _ => 0);
        var positionCounts = Enumerable.Range(0, 10)
            .Select(_ => alphabet.ToDictionary(character => character, _ => 0))
            .ToArray();

        for (var sample = 0; sample < sampleCount; sample++)
        {
            var code = generator.Generate();

            for (var position = 0; position < code.Length; position++)
            {
                var character = code[position];
                characterCounts[character]++;
                positionCounts[position][character]++;
            }
        }

        Assert.True(
            ChiSquared(characterCounts.Values, sampleCount * 10, alphabet.Length) < 150,
            "Overall character frequencies exceeded the broad sanity threshold.");
        foreach (var counts in positionCounts)
        {
            Assert.True(
                ChiSquared(counts.Values, sampleCount, alphabet.Length) < 150,
                "A code position exceeded the broad frequency sanity threshold.");
        }
    }

    [Fact]
    public void Generate_ReportsObservedDuplicatesWithinBroadBirthdayBound()
    {
        const int sampleCount = 100_000;
        const int length = 10;
        const double boundMultiplier = 1_000;
        const string alphabet = "0123456789ABCDEF";

        var generator = new DistributedRandomCodeGenerator(new CompactCodeOptions
        {
            Alphabet = alphabet,
            MinimumLength = length,
            MaximumLength = length
        });
        var observedCodes = new HashSet<string>(sampleCount);
        var duplicateCount = 0;

        for (var sample = 0; sample < sampleCount; sample++)
        {
            if (!observedCodes.Add(generator.Generate()))
            {
                duplicateCount++;
            }
        }

        var codeSpace = Math.Pow(alphabet.Length, length);
        var expectedCollidingPairs = (double)sampleCount * (sampleCount - 1) / (2 * codeSpace);
        var duplicateCountBound = (int)Math.Ceiling(expectedCollidingPairs * boundMultiplier);
        var markovExceedanceBound = expectedCollidingPairs / (duplicateCountBound + 1);

        _output.WriteLine(
            $"Collision observation: samples={sampleCount}, alphabetSize={alphabet.Length}, length={length}, " +
            $"expectedCollidingPairs={expectedCollidingPairs:F6}, observedDuplicateValues={duplicateCount}, " +
            $"broadUpperBound={duplicateCountBound}, markovExceedanceProbabilityBound<{markovExceedanceBound:P4}.");

        Assert.True(
            duplicateCount <= duplicateCountBound,
            $"Observed {duplicateCount} duplicate values, exceeding the broad birthday bound of {duplicateCountBound}.");
    }

    private static double ChiSquared(IEnumerable<int> observedCounts, int total, int categories)
    {
        var expected = (double)total / categories;
        return observedCounts.Sum(observed =>
        {
            var difference = observed - expected;
            return difference * difference / expected;
        });
    }
}
