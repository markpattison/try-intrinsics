using System;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace NetCoreIntrinsics
{
    public class SumProduct
    {
        double[] Data1, Data2;
        int n = 1000000;

        [GlobalSetup]
        public void InitialiseData()
        {
            Data1 = new double[n];
            Data2 = new double[n];

            for (int i = 0; i < n; i++)
            {
                Data1[i] = 10.0 * Math.Sin(i) + Math.Log(i + 10.0);
                Data2[i] = 15.0 * (0.5 + Math.Cos(0.25 * i));
            }
        }

        [Benchmark]
        public double WithLinq()
        {
            return Data1.Zip(Data2, (x1, x2) => x1 * x2).Sum();
        }

        [Benchmark]
        public double AccumulateFor()
        {
            double acc = 0.0;

            for (int i = 0; i < n; i++)
            {
                acc += Data1[i] * Data2[i];
            }

            return acc;
        }

        [Benchmark]
        public double ParallelFor()
        {
            var degrees = 4;

            var sums = new double[degrees];

            Action<int> SumChunk = chunkIndex =>
            {
                double sum = 0.0;

                for (int i = chunkIndex * n / degrees; i < (chunkIndex + 1) * n / degrees; i++)
                {
                    sum += Data1[i] * Data2[i];
                }

                sums[chunkIndex] = sum;
            };

            Parallel.For(0, degrees, SumChunk);

            return sums.Sum();
        }

        [Benchmark]
        public unsafe double WithSse2()
        {
            double result;

            fixed (double* d1 = Data1, d2 = Data2)
            {
                var mul = Vector128<double>.Zero;
                var acc = Vector128<double>.Zero;

                for (int i = 0; i < n; i += 2)
                {
                    mul = Sse2.Multiply(Sse2.LoadVector128(d1 + i), Sse2.LoadVector128(d2 + i));
                    acc = Sse2.Add(acc, mul);
                }

                result = acc.GetElement(0) + acc.GetElement(1);
            }

            return result;
        }

        [Benchmark]
        public unsafe double WithAvx()
        {
            double result;

            fixed (double* d1 = Data1, d2 = Data2)
            {
                var mul = Vector256<double>.Zero;
                var acc = Vector256<double>.Zero;

                for (int i = 0; i < n; i += 4)
                {
                    mul = Avx.Multiply(Avx.LoadVector256(d1 + i), Avx.LoadVector256(d2 + i));
                    acc = Avx.Add(acc, mul);
                }

                result = acc.GetElement(0) + acc.GetElement(1) + acc.GetElement(2) + acc.GetElement(3);
            }

            return result;
        }

        [Benchmark]
        public unsafe double ParallelAvx()
        {
            var degrees = 4;

            var sums = new double[degrees];

            Action<int> SumChunk = chunkIndex =>
            {
                fixed (double* d1 = Data1, d2 = Data2)
                {
                    var mul = Vector256<double>.Zero;
                    var acc = Vector256<double>.Zero;

                    for (int i = chunkIndex * n / degrees; i < (chunkIndex + 1) * n / degrees; i += 4)
                    {
                        mul = Avx.Multiply(Avx.LoadVector256(d1 + i), Avx.LoadVector256(d2 + i));
                        acc = Avx.Add(acc, mul);
                    }

                    sums[chunkIndex] = acc.GetElement(0) + acc.GetElement(1) + acc.GetElement(2) + acc.GetElement(3);
                }
            };

            Parallel.For(0, degrees, SumChunk);

            return sums.Sum();
        }

        static void Main(string[] args)
        {
            var sumProduct = new SumProduct();
            sumProduct.InitialiseData();

            Console.WriteLine($"WithLinq: {sumProduct.WithLinq():F4}");
            Console.WriteLine($"AccumulateFor: {sumProduct.AccumulateFor():F4}");
            Console.WriteLine($"ParallelFor: {sumProduct.ParallelFor():F4}");
            Console.WriteLine($"WithSse2: {sumProduct.WithSse2():F4}");
            Console.WriteLine($"WithAvx: {sumProduct.WithAvx():F4}");
            Console.WriteLine($"ParallelAvx: {sumProduct.ParallelAvx():F4}");
            Console.ReadLine();

            BenchmarkRunner.Run<SumProduct>();
        }
    }
}
