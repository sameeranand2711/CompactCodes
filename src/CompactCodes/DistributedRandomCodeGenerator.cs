using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace CompactCodes;

public sealed class DistributedRandomCodeGenerator : ICompactCodeGenerator
{
    private const int AbsoluteMinimumLength = 8;
    private const int AbsoluteMaximumLength = 64;
    private const int MinimumEntropyBits = 40;
    private readonly string _alphabet;
    private readonly int _length;

    public DistributedRandomCodeGenerator()
        : this(new CompactCodeOptions())
    {
    }

    public DistributedRandomCodeGenerator(CompactCodeOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var alphabet = options.Alphabet;
        var minimumLength = options.MinimumLength;
        var maximumLength = options.MaximumLength;

        Validate(alphabet, minimumLength, maximumLength);

        _alphabet = alphabet;
        _length = minimumLength;
    }

    public string Generate()
    {
        var code = new StringBuilder(_length);

        for (var index = 0; index < _length; index++)
        {
            code.Append(_alphabet[RandomNumberGenerator.GetInt32(_alphabet.Length)]);
        }

        return code.ToString();
    }

    private static void Validate(string alphabet, int minimumLength, int maximumLength)
    {
        if (string.IsNullOrEmpty(alphabet))
        {
            throw new ArgumentException("The alphabet must not be null or empty.", nameof(CompactCodeOptions.Alphabet));
        }

        if (minimumLength < AbsoluteMinimumLength || minimumLength > AbsoluteMaximumLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(CompactCodeOptions.MinimumLength),
                minimumLength,
                $"The preferred length must be between {AbsoluteMinimumLength} and {AbsoluteMaximumLength}.");
        }

        if (maximumLength < minimumLength || maximumLength > AbsoluteMaximumLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(CompactCodeOptions.MaximumLength),
                maximumLength,
                $"The maximum length must be at least the preferred length and no greater than {AbsoluteMaximumLength}.");
        }

        var seen = new HashSet<char>();
        foreach (var character in alphabet)
        {
            if (!IsUnreservedAscii(character))
            {
                throw new ArgumentException(
                    "The alphabet may contain only RFC 3986 unreserved ASCII characters.",
                    nameof(CompactCodeOptions.Alphabet));
            }

            if (!seen.Add(character))
            {
                throw new ArgumentException("The alphabet must not contain duplicate characters.", nameof(CompactCodeOptions.Alphabet));
            }
        }

        var codeSpace = BigInteger.Pow(new BigInteger(alphabet.Length), minimumLength);
        if (codeSpace < (BigInteger.One << MinimumEntropyBits))
        {
            throw new ArgumentException(
                $"The alphabet and preferred length must provide at least {MinimumEntropyBits} bits of nominal code-space entropy.",
                nameof(CompactCodeOptions.MinimumLength));
        }
    }

    private static bool IsUnreservedAscii(char character) =>
        char.IsAsciiLetterOrDigit(character) || character is '-' or '.' or '_' or '~';
}
