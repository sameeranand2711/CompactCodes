namespace CompactCodes;

/// <summary>Generates random compact codes without guaranteeing uniqueness.</summary>
public interface ICompactCodeGenerator
{
    /// <summary>Generates a code using the configured alphabet and preferred length.</summary>
    string Generate();
}
