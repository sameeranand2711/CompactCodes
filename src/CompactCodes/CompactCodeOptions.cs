namespace CompactCodes;

public sealed class CompactCodeOptions
{
    public const string Base62Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    public string Alphabet { get; set; } = Base62Alphabet;

    public int MinimumLength { get; set; } = 10;

    public int MaximumLength { get; set; } = 16;
}
