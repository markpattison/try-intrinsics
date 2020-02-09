module TryIntrinsics.Benchmarks

open System
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running

type SumProductComparison () =
    
    [<DefaultValue>] val mutable Data1 : float[]
    [<DefaultValue>] val mutable Data2 : float[]

    [<Params(100000)>]
    [<DefaultValue>] val mutable N : int

    [<GlobalSetup>]
    member this.InitialiseTestData() =
        this.Data1 <- TestData.testData1 this.N
        this.Data2 <- TestData.testData2 this.N

    [<Benchmark>]
    member this.Map2() = SumProduct.map2 this.Data1 this.Data2

    [<Benchmark>]
    member this.AccumulateIter() = SumProduct.accumulateIter this.Data1 this.Data2

    [<Benchmark>]
    member this.AccumulateFor() = SumProduct.accumulateFor this.Data1 this.Data2

    [<Benchmark>]
    member this.NaiveParallelFor() = SumProduct.naiveParallelFor this.Data1 this.Data2

    [<Benchmark>]
    member this.MiddlingParallelFor() = SumProduct.middlingParallelFor this.Data1 this.Data2

    [<Benchmark>]
    member this.SmartParallelFor() = SumProduct.smartParallelFor this.Data1 this.Data2

    [<Benchmark>]
    member this.Sse2() = SumProduct.withSse2 this.Data1 this.Data2

    [<Benchmark>]
    member this.Sse2B() = SumProduct.withSse2B this.Data1 this.Data2

    [<Benchmark>]
    member this.Sse3() = SumProduct.withSse3 this.Data1 this.Data2

    [<Benchmark>]
    member this.Avx() = SumProduct.withAvx this.Data1 this.Data2

    [<Benchmark>]
    member this.Avx2() = SumProduct.withAvx2 this.Data1 this.Data2

    [<Benchmark>]
    member this.Fma() = SumProduct.withFma this.Data1 this.Data2

[<EntryPoint>]
let Main _ =
    let summary = BenchmarkRunner.Run<SumProductComparison>()
    //printfn "%A" summary
    0
    