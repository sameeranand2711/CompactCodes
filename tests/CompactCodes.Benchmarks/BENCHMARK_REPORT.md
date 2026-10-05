# CompactCodes local benchmark report

**Run date:** 2026-10-05
**Purpose:** Local performance evidence for SPEC.md sections 15 and 20. These results are observations, not a cryptographic certification.

## Environment

- OS: Windows 10.0.26200, x64
- .NET SDK: 10.0.101
- Benchmark runtime: .NET 8.0.29
- Logical processors: 4
- Server GC: disabled
- Benchmark project target: `net8.0`

## Command and configuration

Run from the repository root:

```powershell
dotnet run --project tests\CompactCodes.Benchmarks\CompactCodes.Benchmarks.csproj --configuration Release
```

The dependency-free console harness warms each generator with 5,000 calls, then measures 200,000 serial generations and 200,000 parallel generations per configuration. The full GC is collected before the serial measurement. Parallelism is capped at `Environment.ProcessorCount` (4 here).

Each length (8, 10, 12, 16, and 32) is measured with both Base62 and the smaller custom alphabet `ABCDEFGHJKLMNPQRSTUVWXYZ23456789` (Base32). The Base32 alphabet has 32 distinct symbols, so length 8 meets the library's 40-bit minimum entropy floor. Serial managed allocation is measured using `GC.GetAllocatedBytesForCurrentThread`; parallel allocation is not measured. Each cell is a single timed pass, with no claim of statistical benchmark rigor.

## Results

| Alphabet | Length | Serial codes/sec | Serial allocated bytes/code | Parallel codes/sec |
|---|---:|---:|---:|---:|
| Base62 | 8 | 527,871 | 128.00 | 807,513 |
| Base62 | 10 | 360,019 | 144.00 | 1,083,845 |
| Base62 | 12 | 425,646 | 144.00 | 830,812 |
| Base62 | 16 | 200,882 | 160.00 | 556,681 |
| Base62 | 32 | 175,950 | 224.00 | 277,988 |
| Base32 | 8 | 412,147 | 128.00 | 914,165 |
| Base32 | 10 | 255,918 | 144.00 | 810,898 |
| Base32 | 12 | 255,866 | 144.00 | 541,704 |
| Base32 | 16 | 254,095 | 160.00 | 580,758 |
| Base32 | 32 | 120,436 | 224.00 | 317,558 |

## Limitations

Results depend on this machine, runtime, background load, JIT state, and single-pass measurement order; reruns may differ. Allocation figures are serial per-thread managed allocations, not total process or parallel allocations. This harness is a lightweight regression/performance observation tool, not BenchmarkDotNet and not a cryptographic certification. The benchmark does not justify changing the CSPRNG or symbol-selection algorithm.
