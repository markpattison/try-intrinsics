using System;
using System.Linq;
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

        static void Main(string[] args)
        {
            var sumProduct = new SumProduct();
            sumProduct.InitialiseData();

            Console.WriteLine($"WithLinq: {sumProduct.WithLinq():F4}");
            Console.WriteLine($"AccumulateFor: {sumProduct.AccumulateFor():F4}");
            Console.WriteLine($"ParallelFor: {sumProduct.ParallelFor():F4}");
            Console.ReadLine();

            BenchmarkRunner.Run<SumProduct>();
        }
    }
}
