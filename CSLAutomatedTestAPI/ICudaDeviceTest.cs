using System;
using System.Linq;
using System.Collections.Generic;
using CudaSharper;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace CslAutomatedTestApi
{
    /// <summary>
    /// Tests a single CUDA-enabled device. Tests are independent of eachother, and therefore
    /// can be configured to run in parallel or sequentially. Running in parallel will utilize
    /// all of the CPU cores and will try to call GPU kernels as fast as possible. Each test is
    /// a list of test results. Test results are a list of test records. Test records have a name,
    /// a condition, and the value that is being tested. This allows, for example, a test for
    /// generating a normal distribution to have multiple test records. Record 1 could test the
    /// error code, record 2 could test if there are repeating elements, record 3 could test something else.
    /// The test results for normal distribution would be records 1, 2, and 3.
    /// </summary>
    public abstract class ICudaDeviceTest : ICudaTestParameters, IGpuPerformanceMetrics
    {
        // Coding Guidelines:
        // - ALL Properties should be immutable after construction!
        // - ALL Functions should be pure!
        // - The test should be run on object construction, if they want to delay it, then they can use Lazy<>
        // - Composition over inheritance: inherit interfaces, implement through private classes.

        #region ICudaTestParameters
        private ICudaTestParameters TestParameters { get; }
        public int NumOfTests => TestParameters.NumOfTests;
        public int Range => TestParameters.Range;
        public int DeviceId => TestParameters.DeviceId;
        public CudaTestParallelism DegreeOfParallelism => TestParameters.DegreeOfParallelism;
        #endregion

        #region IGpuPerformanceMetrics
        public long GpuElapsedMilliseconds => Results.GpuElapsedMilliseconds;
        #endregion

        private CudaTestResultCollection _results { get; set; }
        public CudaTestResultCollection Results => _results;

        private static object locker = new object();
        private bool _isInitialized = false;

        protected delegate CudaTestResultCollection CudaTestImplementation(ICudaTestParameters parameters);
        protected virtual Dictionary<CudaTestParallelism, CudaTestImplementation> CudaTestMethod { get; }

        protected IProgress<double> Progression { get; }

        public ICudaDeviceTest(ICudaTestParameters parameters, IProgress<double> progress)
        {
            TestParameters = parameters;

            CudaTestMethod = new Dictionary<CudaTestParallelism, CudaTestImplementation>()
            {
                { CudaTestParallelism.DoNotParallelizeTests, RunTestsInSequential },
                { CudaTestParallelism.DoParallelizeTests, RunTestsInParallel }
            };

            Progression = progress;
        }

        protected bool PercentageErrorCheck(double expected_value, double actual_value, double tolerance = 1e-05)
        {
            return Math.Abs((expected_value - actual_value) / actual_value) < tolerance;
        }

        protected void Initialize()
        {
            lock (locker)
            {
                if (!_isInitialized)
                {
                    _results = CudaTestMethod[TestParameters.DegreeOfParallelism](TestParameters);
                    _isInitialized = true;
                }
            }
        }

        protected bool CudaErrorCodeTest(CudaError error)
        {
            return error == CudaError.Success;
        }

        protected CudaTestResultCollection RunTestsInParallel(ICudaTestParameters parameters)
        {
            lock (locker)
            {
                var testResultsList = new ConcurrentBag<CudaTestRecordCollection>();
                var totalSessionTime = 0L;
                var progression = 0;

                // The Gpu timer keeps track of each GPU function.
                // However, in this case, there is an instance of a stopwatch for each thread,
                // because these tests are running on seperate threads. If you have 4 threads,
                // the result of totalGpuTime is 4x the actual result. Parallel.For does not say
                // how many threads it uses, so this is a "hack" to figure that out.
                var totalTimer = Stopwatch.StartNew();

                Parallel.For(0, parameters.NumOfTests, i =>
                {
                    var timer = Stopwatch.StartNew();
                    testResultsList.Add(TestCsl(parameters));
                    timer.Stop();
                    Interlocked.Add(ref totalSessionTime, timer.ElapsedMilliseconds);

                    if(Progression != null)
                    {
                        Interlocked.Increment(ref progression);
                        Progression.Report((double)progression / parameters.NumOfTests);
                    }
                });

                totalTimer.Stop();
                var threads = (long)Math.Round((double)totalSessionTime / totalTimer.ElapsedMilliseconds);

                return new CudaTestResultCollection(testResultsList.ToList(), parameters)
                {
                    TotalElapsedMilliseconds = totalSessionTime / threads,
                    GpuElapsedMilliseconds = testResultsList.Sum(x => x.GpuElapsedMilliseconds) / threads
                };
            }
        }

        protected CudaTestResultCollection RunTestsInSequential(ICudaTestParameters parameters)
        {
            lock (locker)
            {
                var testResultsList = new List<CudaTestRecordCollection>();
                var timer = Stopwatch.StartNew();

                for (int i = 0; i < parameters.NumOfTests; i++)
                {
                    testResultsList.Add(TestCsl(parameters));
                    if(Progression != null)
                    {
                        Progression.Report((double)i / parameters.NumOfTests);
                    }
                }

                timer.Stop();

                return new CudaTestResultCollection(testResultsList, parameters)
                {
                    TotalElapsedMilliseconds = timer.ElapsedMilliseconds,
                    GpuElapsedMilliseconds = testResultsList.Sum(x => x.GpuElapsedMilliseconds)
                };
            }
        }

        protected abstract CudaTestRecordCollection TestCsl(ICudaTestParameters parameters);
    }
}
