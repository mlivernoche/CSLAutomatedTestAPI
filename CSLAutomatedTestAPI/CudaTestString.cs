using System.Text;

namespace CslAutomatedTestApi
{
    /// <summary>
    /// Takes the results of a test and makes the results readable for humans.
    /// </summary>
    public sealed class CudaTestString
    {
        private StringBuilder Output { get; }

        public CudaTestString(CudaTestResultCollection results)
        {
            Output = new StringBuilder(1024);
            Output.Append("\r");
            Output.AppendLine(new string('-', 50));
            Output.AppendLine($"DeviceId: {results.TestParametersUsed.DeviceId}");
            Output.AppendLine($"Degree of Parallelism: {results.TestParametersUsed.DegreeOfParallelism}\n");
            Output.AppendLine($"Passed: {results.AllTestsPassed.ToString()}");
            Output.AppendLine(
                string.Format(
                    "\n{0,-10}{1,-10}{2,-10}{3,-10}",
                    string.Empty, "Total", "Passes", "Fails"));
            Output.AppendLine(
                string.Format(
                    "{0,-10}{1,-10}{2,-10}{3,-10}",
                    "Tests:", results.Count, results.NumOfPasses, results.NumOfFails));
            Output.AppendLine(
                string.Format(
                    "\n{0,-10}{1,-15}{2,-10}", string.Empty, "Total", "GPU"));
            Output.AppendLine(
                string.Format(
                    "{0,-10}{1,-15}{2,-10}",
                    "Session:",
                    results.TotalElapsedMilliseconds.ToString() + " ms.",
                    results.GpuElapsedMilliseconds.ToString() + " ms."));
            Output.AppendLine(
                string.Format(
                    "{0,-10}{1,-15}{2,-10}",
                    "Best:",
                    CudaTestResultCollection.BestTime.ToString() + " ms.",
                    CudaTestResultCollection.GpuBestTime.ToString() + " ms."));
            Output.Append(new string('-', 50));
        }

        public override string ToString()
        {
            return Output.ToString();
        }
    }
}
