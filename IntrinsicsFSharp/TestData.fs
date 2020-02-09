module TryIntrinsics.TestData

let testData1 n =
    Array.init n (fun i -> 10.0 * sin (float i) )

let testData2 n =
    Array.init n (fun i -> 15.0 * (0.5 + cos (0.25 * float i)))

