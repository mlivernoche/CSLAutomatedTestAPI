using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CslAutomatedTestApi
{
    public class CudaTestResultCollection : IEnumerable<CudaTestRecordCollection>, IGpuPerformanceMetrics
    {
        private IList<CudaTestRecordCollection> TestResults { get; }

        public ICudaTestParameters TestParametersUsed { get; }
        public CudaTestRecordCollection this[int i] => TestResults[i];

        // Put the following chunks in the constructor, because they can be expensive.
        public bool AllTestsPassed { get; }
        public IEnumerable<CudaTestRecordCollection> PassedTests { get; }
        public IEnumerable<CudaTestRecordCollection> FailedTests { get; }
        public int NumOfPasses { get; }
        public int NumOfFails { get; }
        public int Count { get; }

        private static object _lock = new object();

        private static long _bestTime = long.MaxValue;
        public static long BestTime
        {
            get
            {
                lock (_lock)
                {
                    return _bestTime;
                }
            }
            private set
            {
                lock (_lock)
                {
                    _bestTime = value;
                }
            }
        }

        private long _testTime = 0;
        public long TotalElapsedMilliseconds
        {
            get
            {
                return _testTime;
            }
            set
            {
                if (BestTime > value)
                {
                    BestTime = value;
                }
                _testTime = value;
            }
        }

        private static long _gpuBestTime = long.MaxValue;
        public static long GpuBestTime
        {
            get
            {
                lock (_lock)
                {
                    return _gpuBestTime;
                }
            }
            private set
            {
                lock (_lock)
                {
                    _gpuBestTime = value;
                }
            }
        }

        private long _gpuTestTime = 0;
        public long GpuElapsedMilliseconds
        {
            get
            {
                return _gpuTestTime;
            }
            set
            {
                if (GpuBestTime > value)
                {
                    GpuBestTime = value;
                }
                _gpuTestTime = value;
            }
        }

        public CudaTestResultCollection(IList<CudaTestRecordCollection> results, ICudaTestParameters parameters)
        {
            TestResults = results;
            TestParametersUsed = parameters;

            PassedTests = from result in TestResults
                          where result.Passed == true
                          select result;
            NumOfPasses = PassedTests.Count();

            FailedTests = from result in TestResults
                          where result.Passed == false
                          select result;
            NumOfFails = FailedTests.Count();

            AllTestsPassed = !(NumOfFails > 0);
            Count = TestResults.Count;
        }

        public IEnumerator<CudaTestRecordCollection> GetEnumerator()
        {
            foreach(var recordcollection in TestResults)
            {
                yield return recordcollection;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
