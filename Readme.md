On my home PC (Ryzen 7 3700X):

.NET Framework 4.8

| WithLinq | 11,385 us |
| AccumulateFor | 1,205 us |
| ParallelFor | 318 us |

.NET Core 3.1

| WithLinq | 12,781 us |
| AccumulateFor | 721 us |
| ParallelFor | 208 us |
| WithSse2 | 362 us |
| WithAvx | 189 us |
| ParallelAvx | 73 us |
