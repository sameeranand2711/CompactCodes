using CompactCodes;

namespace CompactCodes.Tests;

public sealed class CompactCodeStatisticalTests
{
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
