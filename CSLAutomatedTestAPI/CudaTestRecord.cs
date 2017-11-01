using CudaSharper;

namespace CslAutomatedTestApi
{
    public class CudaTestRecord : IGpuPerformanceMetrics
    {
        public string Name { get; }
        public string Value { get; }
        public bool PassedTest { get; }
        public CudaError ErrorCode { get; }
        public bool GpuReturnedSuccess => ErrorCode == CudaError.Success;

        #region IGpuPerformanceMetrics
        private IGpuPerformanceMetrics GpuMetrics { get; }
        public long GpuElapsedMilliseconds => GpuMetrics.GpuElapsedMilliseconds;
        #endregion

        private CudaTestRecord(string name, string value, bool test, CudaError errorcode, IGpuPerformanceMetrics gpu_metrics)
        {
            Name = name;
            Value = value;
            PassedTest = test;
            ErrorCode = errorcode;
            GpuMetrics = gpu_metrics;
        }

        public static CudaTestRecord New<T>(string name, T value, bool test, CudaError errorcode, IGpuPerformanceMetrics gpu_metrics)
        {
            return new CudaTestRecord(name, value.ToString(), test, errorcode, gpu_metrics);
        }
    }
}
