using System;

namespace CslAutomatedTestApi
{
    public interface IProgressToCompletion
    {
        double Percent { get; }
        TimeSpan ReportPeriod { get; }
    }
}
