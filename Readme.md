On my home PC (Ryzen 7 3700X):

**.NET Framework 4.8**

| Method | Time (Ryzen 7 3700X) | Time (i7-6500U) |
| ---- | ---: | ---: |
| WithLinq | 11,385 us | 16,064 us |
| AccumulateFor | 1,205 us | 1,506 us |
| ParallelFor | 318 us | 983 us |

**.NET Core 3.1**

| Method | Time (Ryzen 7 3700X) | Time (i7-6500U) |
| --- | ---: | ---: |
| WithLinq | 12,781 us | 18,659 us |
| AccumulateFor | 712 us | 1,518 us |
| ParallelFor | 197 us | 996 us |
| WithSse2 | 355 us | 1,129 us |
| WithAvx | 182 us | 1,059 us |
| ParallelAvx | 57 us | 986 us |

Rough plan for presentation:

1. Introduction
    - performance of numerical calculations
    - .NET focused
    - not appropriate for all situations
2. SumProduct
    - cf. discounted cashflows
    - caveat: haven't allowed for odd elements at the end
    - used my home PC (closer to a server than my work laptop)
    - take a guess! (assume max 4 cores)
3. BenchmarkDotNet
    - avoids common pitfalls
    - automates everything
    - use Release mode
4. Start with .NET Framework
    - WithLinq
    - AccumulateFor
    - ParallelFor
5. Move to .NET Core
    - speedup
6. Intro to hardware intrinsics
    - basic idea/motivation
    - history
    - caveat: `unsafe`
    - caveat: haven't checked for compatibility
7. Intrinsics results
    - SSE2
    - AVX
    - Parallel AVX
8. Conclusions
    - Linq can be very slow
    - simple for loop often good enough
    - not hard to parallelise
    - .NET Core can make a big difference
    - very different results on different hardware
    - intrinsics for extreme cases
    - I need a new laptop
    - (repo link)
