On my home PC (Ryzen 7 3700X):

.NET Framework 4.8

| Method | Time |
| ---- | ---: |
| WithLinq | 11,385 us |
| AccumulateFor | 1,205 us |
| ParallelFor | 318 us |

.NET Core 3.1

| Method | Time |
| --- | ---: |
| WithLinq | 12,781 us |
| AccumulateFor | 721 us |
| ParallelFor | 208 us |
| WithSse2 | 362 us |
| WithAvx | 189 us |
| ParallelAvx | 73 us |

Rough plan for presentation:

1. Introduction
    - performance of numerical calculations
    - .NET focused
    - not appropriate for all situations
2. SumProduct
    - cf. discounted cashflows
    - caveat: haven't allowed for odd elements at the end
3. BenchmarkDotNet
    - avoids common pitfalls
    - automates everything
4. Start with .NET Framework
    - WithLinq
    - AccumulateFor
    - ParallelFor
5. Move to .NET Core
    - speedup
6. Intro to hardware intrinsics
    - basic idea/motivation
    - history
7. Intrinsics results
    - SSE2
    - AVX
    - Parallel AVX
8. Conclusions
    - Linq can be very slow
    - simple for loop often good enough
    - not hard to parallelise
    - intrinsics for extreme cases
