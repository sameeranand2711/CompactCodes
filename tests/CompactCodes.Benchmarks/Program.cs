using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using CompactCodes;

const int iterations = 200_000;
const int warmupIterations = 5_000;
const string smallerAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
var lengths = new[] { 8, 10, 12, 16, 32 };
var configurations = new[]
{
    (Name: "Base62", Alphabet: CompactCodeOptions.Base62Alphabet),
    (Name: "Base32", Alphabet: smallerAlphabet)
};

Console.WriteLine($"Runtime: {RuntimeInformation.FrameworkDescription}");
Console.WriteLine($"OS: {RuntimeInformation.OSDescription} ({RuntimeInformation.ProcessArchitecture})");
Console.WriteLine($"Logical processors: {Environment.ProcessorCount}");
Console.WriteLine($"Server GC: {GCSettings.IsServerGC}");
Console.WriteLine($"Iterations per measurement: {iterations:N0}");
Console.WriteLine($"Warmup per configuration: {warmupIterations:N0}");
Console.WriteLine($"Parallelism: {Environment.ProcessorCount}");
Console.WriteLine();
Console.WriteLine("Alphabet,Length,Serial codes/sec,Serial allocated bytes/code,Parallel codes/sec");

foreach (var (name, alphabet) in configurations)
{
    foreach (var length in lengths)
    {
        var generator = new DistributedRandomCodeGenerator(new CompactCodeOptions
        {
            Alphabet = alphabet,
            MinimumLength = length,
            MaximumLength = length
        });

        WarmUp(generator, warmupIterations);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        var serialTimer = Stopwatch.StartNew();
        long serialChecksum = 0;
        for (var index = 0; index < iterations; index++)
        {
            serialChecksum += generator.Generate()[0];
        }
        serialTimer.Stop();
        var allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;

        var parallelTimer = Stopwatch.StartNew();
        long parallelChecksum = 0;
        Parallel.For(
            0,
            iterations,
            () => 0L,
            (_, _, localChecksum) => localChecksum + generator.Generate()[0],
            localChecksum => Interlocked.Add(ref parallelChecksum, localChecksum));
        parallelTimer.Stop();

        GC.KeepAlive(serialChecksum);
        GC.KeepAlive(parallelChecksum);

        var serialCodesPerSecond = iterations / serialTimer.Elapsed.TotalSeconds;
        var parallelCodesPerSecond = iterations / parallelTimer.Elapsed.TotalSeconds;
        var allocatedBytesPerCode = (double)allocatedBytes / iterations;

        Console.WriteLine(
            $"{name},{length},{serialCodesPerSecond:F0},{allocatedBytesPerCode:F2},{parallelCodesPerSecond:F0}");
    }
}

static void WarmUp(ICompactCodeGenerator generator, int count)
{
    long checksum = 0;
    for (var index = 0; index < count; index++)
    {
        checksum += generator.Generate()[0];
    }

    GC.KeepAlive(checksum);
}
