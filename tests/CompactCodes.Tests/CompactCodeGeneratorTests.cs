using System.Collections.Concurrent;
using CompactCodes;

namespace CompactCodes.Tests;

public sealed class CompactCodeGeneratorTests
{
    [Fact]
    public void Generate_UsesBase62AndDefaultLength()
    {
        var generator = new DistributedRandomCodeGenerator();
        var code = generator.Generate();

        Assert.Equal(10, code.Length);
        Assert.All(code, character => Assert.Contains(character, CompactCodeOptions.Base62Alphabet));
    }

    [Fact]
    public void Generate_UsesCustomAlphabetAndPreferredLength()
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var generator = new DistributedRandomCodeGenerator(new CompactCodeOptions
        {
            Alphabet = alphabet,
            MinimumLength = 12,
            MaximumLength = 16
        });

        var code = generator.Generate();

        Assert.Equal(12, code.Length);
        Assert.All(code, character => Assert.Contains(character, alphabet));
    }

    [Theory]
    [InlineData("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ")]
    [InlineData("0123456789abcdefghijklmnopqrstuvwxyz")]
    public void Generate_PreservesConfiguredAlphabetCase(string alphabet)
    {
        var generator = new DistributedRandomCodeGenerator(new CompactCodeOptions
        {
            Alphabet = alphabet,
            MinimumLength = 8,
            MaximumLength = 8
        });

        for (var sample = 0; sample < 100; sample++)
        {
            var code = generator.Generate();
            Assert.All(code, character => Assert.Contains(character, alphabet));
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("aa")]
    [InlineData("abc/defghijklmnopqrstuvwxyz0123456789")]
    [InlineData("abcdefghijklmno\u00e9qrstuvwxyz0123456789")]
    public void Constructor_RejectsInvalidAlphabets(string alphabet)
    {
        var options = new CompactCodeOptions { Alphabet = alphabet };

        Assert.Throws<ArgumentException>(() => new DistributedRandomCodeGenerator(options));
    }

    [Fact]
    public void Constructor_RejectsNullAlphabet()
    {
        var options = new CompactCodeOptions { Alphabet = null! };

        Assert.Throws<ArgumentException>(() => new DistributedRandomCodeGenerator(options));
    }

    [Fact]
    public void Constructor_RejectsNullOptions()
    {
        Assert.Throws<ArgumentNullException>(() => new DistributedRandomCodeGenerator(null!));
    }

    [Theory]
    [InlineData(7, 16)]
    [InlineData(65, 65)]
    [InlineData(10, 9)]
    [InlineData(10, 65)]
    public void Constructor_RejectsOutOfRangeLengths(int minimumLength, int maximumLength)
    {
        var options = new CompactCodeOptions
        {
            MinimumLength = minimumLength,
            MaximumLength = maximumLength
        };

        Assert.Throws<ArgumentOutOfRangeException>(() => new DistributedRandomCodeGenerator(options));
    }

    [Fact]
    public void Constructor_RejectsAlphabetAndLengthBelowEntropyFloor()
    {
        var options = new CompactCodeOptions
        {
            Alphabet = "0123456789",
            MinimumLength = 8,
            MaximumLength = 64
        };

        Assert.Throws<ArgumentException>(() => new DistributedRandomCodeGenerator(options));
    }

    [Fact]
    public void Constructor_AcceptsHardLengthBoundaries()
    {
        var minimum = new DistributedRandomCodeGenerator(new CompactCodeOptions
        {
            MinimumLength = 8,
            MaximumLength = 8
        });
        var maximum = new DistributedRandomCodeGenerator(new CompactCodeOptions
        {
            MinimumLength = 64,
            MaximumLength = 64
        });

        Assert.Equal(8, minimum.Generate().Length);
        Assert.Equal(64, maximum.Generate().Length);
    }

    [Fact]
    public void Constructor_AcceptsExactlyFortyBitsOfNominalEntropy()
    {
        var generator = new DistributedRandomCodeGenerator(new CompactCodeOptions
        {
            Alphabet = "01",
            MinimumLength = 40,
            MaximumLength = 40
        });

        Assert.Equal(40, generator.Generate().Length);
    }

    [Fact]
    public void Generate_IsSafeForConcurrentCalls()
    {
        const int generationCount = 1_000_000;
        var generator = new DistributedRandomCodeGenerator();
        var invalidCodes = new ConcurrentQueue<string>();

        Parallel.For(0, generationCount, _ =>
        {
            var code = generator.Generate();
            if (code.Length != 10 || code.Any(character => !CompactCodeOptions.Base62Alphabet.Contains(character)))
            {
                invalidCodes.Enqueue(code);
            }
        });

        Assert.Empty(invalidCodes);
    }
}
