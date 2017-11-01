using System;

namespace CslAutomatedTestApi
{
    public interface ICudaTestParameters
    {
        int NumOfTests { get; }
        int Range { get; }
        int DeviceId { get; }
        CudaTestParallelism DegreeOfParallelism { get; }
    }
}
