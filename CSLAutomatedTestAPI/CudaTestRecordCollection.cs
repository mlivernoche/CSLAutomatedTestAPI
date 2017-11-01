using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace CslAutomatedTestApi
{
    public class CudaTestRecordCollection : IEnumerable<CudaTestRecord>, IGpuPerformanceMetrics
    {
        private IList<CudaTestRecord> _cudaTestRecordList { get; }

        public CudaTestRecord this[int i] => _cudaTestRecordList[i];
        public int Count => _cudaTestRecordList.Count;
        public bool Passed { get; }

        #region IGpuPerformanceMetrics
        private IGpuPerformanceMetrics GpuMetrics { get; }
        public long GpuElapsedMilliseconds => GpuMetrics.GpuElapsedMilliseconds;
        #endregion

        public CudaTestRecordCollection(IList<CudaTestRecord> recordcollection, IGpuPerformanceMetrics metrics)
        {
            _cudaTestRecordList = recordcollection;
            GpuMetrics = metrics;

            // Null Hypothesis: We passed the test. Find any proof that we failed.
            Passed = !_cudaTestRecordList.Any(record => !record.PassedTest && !record.GpuReturnedSuccess);
        }

        public IEnumerator<CudaTestRecord> GetEnumerator()
        {
            foreach(var record in _cudaTestRecordList)
            {
                yield return record;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
