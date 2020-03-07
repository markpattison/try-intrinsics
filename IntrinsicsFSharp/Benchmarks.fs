module TryIntrinsics.Benchmarks

open System
open System.Runtime.Intrinsics
open System.Runtime.Intrinsics.X86
open System.Threading.Tasks

open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running

#nowarn "9"

type SumProductComparison () =
    
    [<DefaultValue>] val mutable Data1 : float[]
    [<DefaultValue>] val mutable Data2 : float[]

    let n = 1000000

    [<GlobalSetup>]
    member this.InitialiseTestData() =
        this.Data1 <- Array.init n (fun i -> 10.0 * sin (float i) + log (float i + 10.0))
        this.Data2 <- Array.init n (fun i -> 15.0 * (0.5 + cos (0.25 * float i)))

    [<Benchmark>]
    member this.Map2() =
        this.Data1
        |> Array.map2 (*) this.Data2
        |> Array.sum

    [<Benchmark>]
    member this.AccumulateIter() =
        let mutable acc = 0.0

        this.Data1
        |> Array.iteri (fun n x ->
            acc <- acc + x * this.Data2.[n])
        acc

    [<Benchmark>]
    member this.AccumulateFor() =
        let mutable acc = 0.0

        for i in 0 .. (n - 1) do
            acc <- acc + this.Data1.[i] * this.Data2.[i]
        acc

    [<Benchmark>]
    member this.NaiveParallelFor() =
        let sums = Array.zeroCreate n

        Parallel.For(0, n, fun i ->
            sums.[i] <- this.Data1.[i] * this.Data2.[i]) |> ignore

        Array.sum sums

    [<Benchmark>]
    member this.MiddlingParallelFor() =
        let degrees = 4
        let sums = Array.zeroCreate degrees

        Parallel.For(0, degrees, fun p ->
            let mutable acc = 0.0

            for i in p .. degrees .. (n - 1)  do
                acc <- acc + this.Data1.[i] * this.Data2.[i]
    
            sums.[p] <- acc) |> ignore

        let mutable acc = 0.0
        for i in 0 .. (sums.Length - 1) do
            acc <- acc + sums.[i]
        acc

    [<Benchmark>]
    member this.SmartParallelFor() =
        let degrees = 4
        let sums = Array.zeroCreate degrees

        Parallel.For(0, degrees, fun p ->
            let mutable acc = 0.0
            let startIndex = n * p / degrees
            let endIndex =
                if p = degrees - 1 then
                    n - 1
                else
                    n * (p + 1) / degrees - 1

            for i in startIndex .. endIndex do
                acc <- acc + this.Data1.[i] * this.Data2.[i]
    
            sums.[p] <- acc) |> ignore

        let mutable acc = 0.0
        for i in 0 .. (sums.Length - 1) do
            acc <- acc + sums.[i]
        acc

    [<Benchmark>]
    member this.Sse2() =
        if not Sse2.IsSupported then failwith "SSE2 not available"

        use d1 = fixed this.Data1
        use d2 = fixed this.Data2

        let mutable accV = Vector128<float>.Zero // holds two double-precision floats
        let mutable mulV = Vector128<float>.Zero

        let lastIndex = n - 2 - (n % 2)
        
        for i in 0 .. 2 .. lastIndex do
            mulV <- Sse2.Multiply(Sse2.LoadVector128(NativeInterop.NativePtr.add d1 i), Sse2.LoadVector128(NativeInterop.NativePtr.add d2 i))
            accV <- Sse2.Add(accV, mulV)

        let mutable acc = accV.GetElement(0) + accV.GetElement(1)

        // any remaining elements
        for i in lastIndex + 2 .. n - 1 do
            acc <- acc + this.Data1.[i] * this.Data2.[i]

        acc

    [<Benchmark>]
    member this.Sse2B() =
        if not Sse2.IsSupported then failwith "SSE2 not available"

        use d1 = fixed this.Data1
        use d2 = fixed this.Data2

        let mutable accV = Vector128<float>.Zero // holds two double-precision floats
        let mutable mulV = Vector128<float>.Zero

        let lastIndex = n - 2 - (n % 2)

        for i in 0 .. 2 .. lastIndex do
            mulV <- Sse2.Multiply(Sse2.LoadVector128(NativeInterop.NativePtr.add d1 i), Sse2.LoadVector128(NativeInterop.NativePtr.add d2 i))
            accV <- Sse2.Add(accV, mulV)

        let mutable acc = accV.GetElement(0) + accV.GetElement(1)

        // any remaining elements
        for i in lastIndex + 2 .. n - 1 do
            acc <- acc + this.Data1.[i] * this.Data2.[i]

        acc

    [<Benchmark>]
    member this.Sse3() =
        if not Sse3.IsSupported then failwith "SSE3 not available"

        use d1 = fixed this.Data1
        use d2 = fixed this.Data2

        let mutable accV = Vector128<float>.Zero // holds two double-precision floats
        let mutable mulV = Vector128<float>.Zero

        let lastIndex = n - 2 - (n % 2)

        for i in 0 .. 2 .. lastIndex do
            mulV <- Sse3.Multiply(Sse3.LoadVector128(NativeInterop.NativePtr.add d1 i), Sse3.LoadVector128(NativeInterop.NativePtr.add d2 i))
            accV <- Sse3.Add(accV, mulV)

        let mutable acc = accV.GetElement(0) + accV.GetElement(1)

        // any remaining elements
        for i in lastIndex + 2 .. n - 1 do
            acc <- acc + this.Data1.[i] * this.Data2.[i]

        acc

    [<Benchmark>]
    member this.Avx() =
        if not Avx.IsSupported then failwith "AVX not available"

        use d1 = fixed this.Data1
        use d2 = fixed this.Data2

        let mutable accV = Vector256<float>.Zero // holds four double-precision floats
        let mutable mulV = Vector256<float>.Zero

        let lastIndex = n - 4 - (n % 4)

        for i in 0 .. 4 .. lastIndex do
            mulV <- Avx.Multiply(Avx.LoadVector256(NativeInterop.NativePtr.add d1 i), Avx.LoadVector256(NativeInterop.NativePtr.add d2 i))
            accV <- Avx.Add(accV, mulV)

        let mutable acc = accV.GetElement(0) + accV.GetElement(1) + accV.GetElement(2) + accV.GetElement(3)

        // any remaining elements
        for i in lastIndex + 4 .. n - 1 do
            acc <- acc + this.Data1.[i] * this.Data2.[i]

        acc

    [<Benchmark>]
    member this.Avx2() =
        if not Avx2.IsSupported then failwith "AVX2 not available"

        use d1 = fixed this.Data1
        use d2 = fixed this.Data2

        let mutable accV = Vector256<float>.Zero // holds four double-precision floats
        let mutable mulV = Vector256<float>.Zero

        let lastIndex = n - 4 - (n % 4)

        for i in 0 .. 4 .. lastIndex do
            mulV <- Avx2.Multiply(Avx2.LoadVector256(NativeInterop.NativePtr.add d1 i), Avx2.LoadVector256(NativeInterop.NativePtr.add d2 i))
            accV <- Avx2.Add(accV, mulV)

        let mutable acc = accV.GetElement(0) + accV.GetElement(1) + accV.GetElement(2) + accV.GetElement(3)

        // any remaining elements
        for i in lastIndex + 4 .. n - 1 do
            acc <- acc + this.Data1.[i] * this.Data2.[i]

        acc

    [<Benchmark>]
    member this.Fma() =
        if not Fma.IsSupported then failwith "FMA not available"

        use d1 = fixed this.Data1
        use d2 = fixed this.Data2

        let mutable accV = Vector256<float>.Zero // holds four double-precision floats
        let mutable mulV = Vector256<float>.Zero

        let lastIndex = n - 4 - (n % 4)

        for i in 0 .. 4 .. lastIndex do
            let a = Fma.LoadVector256(NativeInterop.NativePtr.add d1 i)
            let b = Fma.LoadVector256(NativeInterop.NativePtr.add d2 i)
            accV <- Fma.MultiplyAdd(Fma.LoadVector256(NativeInterop.NativePtr.add d1 i), Fma.LoadVector256(NativeInterop.NativePtr.add d2 i), accV)

        let mutable acc = accV.GetElement(0) + accV.GetElement(1) + accV.GetElement(2) + accV.GetElement(3)

        // any remaining elements
        for i in lastIndex + 4 .. n - 1 do
            acc <- acc + this.Data1.[i] * this.Data2.[i]

        acc

[<EntryPoint>]
let Main _ =
    let spc = SumProductComparison()
    spc.InitialiseTestData()

    // should be 96116568.8346

    printfn "Map2: %.4f" (spc.Map2())
    printfn "AccumulateIter: %.4f" (spc.AccumulateIter())
    printfn "AccumulateFor: %.4f" (spc.AccumulateFor())
    printfn "NaiveParallelFor: %.4f" (spc.NaiveParallelFor())
    printfn "MiddlingParallelFor: %.4f" (spc.MiddlingParallelFor())
    printfn "SmartParallelFor: %.4f" (spc.SmartParallelFor())
    printfn "SSE2: %.4f" (spc.Sse2())
    printfn "SSE2B: %.4f" (spc.Sse2B())
    printfn "SSE3: %.4f" (spc.Sse3())
    printfn "Avx: %.4f" (spc.Avx())
    printfn "Avx2: %.4f" (spc.Avx2())
    printfn "Fma: %.4f" (spc.Fma())
    Console.ReadLine() |> ignore

    let summary = BenchmarkRunner.Run<SumProductComparison>()
    //printfn "%A" summary
    0
    