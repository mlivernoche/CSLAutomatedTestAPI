using CudaSharper;

namespace CslAutomatedTestApi
{
    public class CudaTestParameters : ICudaTestParameters
    {
        public int NumOfTests { get; set; }
        public int Range { get; set; }
        public int DeviceId { get; set; }
        public CudaTestParallelism DegreeOfParallelism { get; set; }
    }
}
