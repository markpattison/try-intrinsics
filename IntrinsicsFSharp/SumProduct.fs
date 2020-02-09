module TryIntrinsics.SumProduct

open System.Threading.Tasks

let map2 (data1: float[]) (data2: float[]) =
    Array.map2 (fun x y -> x * y) data1 data2
    |> Array.sum

let accumulateIter (data1: float[]) (data2: float[]) =
    let mutable acc = 0.0

    data1
    |> Array.iteri (fun n x ->
        acc <- acc + x * data2.[n])
    acc

let accumulateFor (data1: float[]) (data2: float[]) =
    let mutable acc = 0.0

    for i in 0 .. (data1.Length - 1) do
        acc <- acc + data1.[i] * data2.[i]
    acc

let naiveParallelFor (data1: float[]) (data2: float[]) =
    let sums = Array.zeroCreate data1.Length

    Parallel.For(0, data1.Length, fun i ->
        sums.[i] <- data1.[i] * data2.[i]) |> ignore

    Array.sum sums

let middlingParallelFor (data1: float[]) (data2: float[]) =
    let degrees = 4
    let sums = Array.zeroCreate degrees

    Parallel.For(0, degrees, fun p ->
        let mutable acc = 0.0

        for i in p .. degrees .. (data1.Length - 1)  do
            acc <- acc + data1.[i] * data2.[i]
        
        sums.[p] <- acc) |> ignore

    let mutable acc = 0.0
    for i in 0 .. (sums.Length - 1) do
        acc <- acc + sums.[i]
    acc

let smartParallelFor (data1: float[]) (data2: float[]) =
    let degrees = 4
    let sums = Array.zeroCreate degrees

    Parallel.For(0, degrees, fun p ->
        let mutable acc = 0.0
        let startIndex = data1.Length * p / degrees
        let endIndex =
            if p = degrees - 1 then
                data1.Length - 1
            else
                data1.Length * (p + 1) / degrees - 1

        for i in startIndex .. endIndex do
            acc <- acc + data1.[i] * data2.[i]
        
        sums.[p] <- acc) |> ignore

    let mutable acc = 0.0
    for i in 0 .. (sums.Length - 1) do
        acc <- acc + sums.[i]
    acc

open System.Runtime.Intrinsics
open System.Runtime.Intrinsics.X86

let withSse2 (data1: float[]) (data2: float[]) =

    if not Sse2.IsSupported then failwith "SSE2 not available"

    use d1 = fixed data1
    use d2 = fixed data2

    let mutable accV = Vector128<float>.Zero // holds two double-precision floats
    let mutable mulV = Vector128<float>.Zero
    
    let lastIndex = data1.Length - (data1.Length / 2) - 2

    for i in 0 .. 2 .. lastIndex do
        mulV <- Sse2.Multiply(Sse2.LoadVector128(NativeInterop.NativePtr.add d1 i), Sse2.LoadVector128(NativeInterop.NativePtr.add d2 i))
        accV <- Sse2.Add(accV, mulV)

    let mutable acc = accV.GetElement(0) + accV.GetElement(1)

    // any remaining elements
    for i in lastIndex + 1 .. data1.Length - 1 do
        acc <- acc + data1.[i] * data2.[i]

    acc

let withSse2B (data1: float[]) (data2: float[]) =

    if not Sse2.IsSupported then failwith "SSE2 not available"

    use d1 = fixed data1
    use d2 = fixed data2

    let mutable accV = Vector128<float>.Zero // holds two double-precision floats
    let mutable mulV = Vector128<float>.Zero
    
    let lastIndex = data1.Length - (data1.Length / 2) - 2

    for i in 0 .. 2 .. lastIndex do
        mulV <- Sse2.Multiply(Sse2.LoadVector128(NativeInterop.NativePtr.add d1 i), Sse2.LoadVector128(NativeInterop.NativePtr.add d2 i))
        accV <- Sse2.Add(accV, mulV)

    let mutable acc = accV.GetElement(0) + accV.GetElement(1)

    // any remaining elements
    for i in lastIndex + 1 .. data1.Length - 1 do
        acc <- acc + data1.[i] * data2.[i]

    acc

let withSse3 (data1: float[]) (data2: float[]) =

    if not Sse3.IsSupported then failwith "SSE3 not available"

    use d1 = fixed data1
    use d2 = fixed data2

    let mutable accV = Vector128<float>.Zero // holds two double-precision floats
    let mutable mulV = Vector128<float>.Zero
    
    let lastIndex = data1.Length - (data1.Length / 2) - 2

    for i in 0 .. 2 .. lastIndex do
        mulV <- Sse3.Multiply(Sse3.LoadVector128(NativeInterop.NativePtr.add d1 i), Sse3.LoadVector128(NativeInterop.NativePtr.add d2 i))
        accV <- Sse3.Add(accV, mulV)

    let mutable acc = accV.GetElement(0) + accV.GetElement(1)

    // any remaining elements
    for i in lastIndex + 1 .. data1.Length - 1 do
        acc <- acc + data1.[i] * data2.[i]

    acc

let withAvx (data1: float[]) (data2: float[]) =

    if not Avx.IsSupported then failwith "AVX not available"

    use d1 = fixed data1
    use d2 = fixed data2

    let mutable accV = Vector256<float>.Zero // holds four double-precision floats
    let mutable mulV = Vector256<float>.Zero
    
    let lastIndex = data1.Length - (data1.Length / 4) - 4

    for i in 0 .. 4 .. lastIndex do
        mulV <- Avx.Multiply(Avx.LoadVector256(NativeInterop.NativePtr.add d1 i), Avx.LoadVector256(NativeInterop.NativePtr.add d2 i))
        accV <- Avx.Add(accV, mulV)

    let mutable acc = accV.GetElement(0) + accV.GetElement(1) + accV.GetElement(2) + accV.GetElement(3)

    // any remaining elements
    for i in lastIndex + 1 .. data1.Length - 1 do
        acc <- acc + data1.[i] * data2.[i]

    acc

let withAvx2 (data1: float[]) (data2: float[]) =

    if not Avx2.IsSupported then failwith "AVX2 not available"

    use d1 = fixed data1
    use d2 = fixed data2

    let mutable accV = Vector256<float>.Zero // holds four double-precision floats
    let mutable mulV = Vector256<float>.Zero
    
    let lastIndex = data1.Length - (data1.Length / 4) - 4

    for i in 0 .. 4 .. lastIndex do
        mulV <- Avx2.Multiply(Avx2.LoadVector256(NativeInterop.NativePtr.add d1 i), Avx2.LoadVector256(NativeInterop.NativePtr.add d2 i))
        accV <- Avx2.Add(accV, mulV)

    let mutable acc = accV.GetElement(0) + accV.GetElement(1) + accV.GetElement(2) + accV.GetElement(3)

    // any remaining elements
    for i in lastIndex + 1 .. data1.Length - 1 do
        acc <- acc + data1.[i] * data2.[i]

    acc

let withFma (data1: float[]) (data2: float[]) =

    if not Fma.IsSupported then failwith "FMA not available"

    use d1 = fixed data1
    use d2 = fixed data2

    let mutable accV = Vector256<float>.Zero // holds four double-precision floats
    let mutable mulV = Vector256<float>.Zero
    
    let lastIndex = data1.Length - (data1.Length / 4) - 4

    for i in 0 .. 4 .. lastIndex do
        accV <- Fma.MultiplyAdd(accV, Fma.LoadVector256(NativeInterop.NativePtr.add d1 i), Fma.LoadVector256(NativeInterop.NativePtr.add d2 i))

    let mutable acc = accV.GetElement(0) + accV.GetElement(1) + accV.GetElement(2) + accV.GetElement(3)

    // any remaining elements
    for i in lastIndex + 1 .. data1.Length - 1 do
        acc <- acc + data1.[i] * data2.[i]

    acc