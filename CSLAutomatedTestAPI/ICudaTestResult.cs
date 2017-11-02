using System;
using CudaSharper;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

namespace CslAutomatedTestApi
{
    internal interface ICudaTestResult<T> : ICudaResult<T>, IGpuPerformanceMetrics
    {
        CudaTestRecord TestRecord { get; }
    }

    internal class CudaTestResultCuRand<T> : ICudaTestResult<T[]>
    {
        public CudaError Error { get; }
        public T[] Result { get; }
        public CudaTestRecord TestRecord { get; }
        public long GpuElapsedMilliseconds { get; }

        public CudaTestResultCuRand(string name, Func<int, ICudaResult<T[]>> curand, int range)
        {
            var timer = Stopwatch.StartNew();
            var result = curand(range);
            timer.Stop();
            GpuElapsedMilliseconds = timer.ElapsedMilliseconds;

            Error = result.Error;
            Result = result.Result;

            // This used to be .Distinct.Count() / .Length.
            // The purpose is to see if the RNG produced repeating numbers, mostly due to programmer error.
            // Using this method, testing for uniqueness is about 5x as fast (in total).
            // Doing a test of 15 iterations and 1,000,000 numbers for each iterations,
            // using the old method took 45744ms on an i5-4460 with a GTX 1070 and a GTX 1060 3GB.
            // Doing it this way, the test takes 9932ms on the same test configuration and system.
            // This is at 1000 samples/sec. Total test diagonstics session was ~10 seconds shorter.
            var duplicates = 0;
            Parallel.For(0, result.Result.Length - 1, i =>
            {
                if (result.Result[i].Equals(result.Result[i + 1]))
                {
                    Interlocked.Increment(ref duplicates);
                }
            });

            // duplicates will never be higher than result.Result.Length, and duplicates is an increasing variable,
            // so we have to test for it being low.
            var uniqueness = (double)duplicates / result.Result.Length;

            TestRecord = CudaTestRecord.New(
                name + "<5% duplicates",
                uniqueness,
                uniqueness < 0.05f,
                result.Error,
                new GpuPerformanceMetrics(GpuElapsedMilliseconds));
        }
    }
    
    internal class CudaTestResultcuStats<T, R> : ICudaTestResult<R>
    {
        public CudaError Error { get; }
        public R Result { get; }
        public CudaTestRecord TestRecord { get; }
        public long GpuElapsedMilliseconds { get; }

        public CudaTestResultcuStats(string name, Func<R, bool> successTest, Func<T[], ICudaResult<R>> testMethod, T[] data)
        {
            var timer = Stopwatch.StartNew();
            var result = testMethod(data);
            timer.Stop();
            GpuElapsedMilliseconds = timer.ElapsedMilliseconds;

            Result = result.Result;
            Error = result.Error;

            TestRecord = CudaTestRecord.New(
                name,
                result.Result,
                successTest(result.Result),
                result.Error,
                new GpuPerformanceMetrics(GpuElapsedMilliseconds));
        }

        public CudaTestResultcuStats(string name, Func<R, bool> successTest, Func<T[], T[], ICudaResult<R>> testMethod, T[] data1, T[] data2)
        {
            var timer = Stopwatch.StartNew();
            var result = testMethod(data1, data2);
            timer.Stop();
            GpuElapsedMilliseconds = timer.ElapsedMilliseconds;

            Result = result.Result;
            Error = result.Error;

            TestRecord = CudaTestRecord.New(
                name,
                result.Result,
                successTest(result.Result),
                result.Error,
                new GpuPerformanceMetrics(GpuElapsedMilliseconds));
        }
    }
}
