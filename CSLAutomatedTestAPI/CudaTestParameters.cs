namespace CslAutomatedTestApi
{
    public sealed class CudaTestParameters : ICudaTestParameters
    {
        public int NumOfTests { get; set; }
        public int Range { get; set; }
        public int DeviceId { get; set; }
        public CudaTestParallelism DegreeOfParallelism { get; set; }
    }
}
