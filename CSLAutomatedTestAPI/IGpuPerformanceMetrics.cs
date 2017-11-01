namespace CslAutomatedTestApi
{
    public interface IGpuPerformanceMetrics
    {
        long GpuElapsedMilliseconds { get; }
    }

    public struct GpuPerformanceMetrics : IGpuPerformanceMetrics
    {
        public long GpuElapsedMilliseconds { get; }

        public GpuPerformanceMetrics(long gputime)
        {
            GpuElapsedMilliseconds = gputime;
        }
    }
}
