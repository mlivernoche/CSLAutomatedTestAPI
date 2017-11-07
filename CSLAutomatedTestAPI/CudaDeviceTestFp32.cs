using System.Linq;
using System.Collections.Generic;
using CudaSharper;
using System;

namespace CslAutomatedTestApi
{
    /// <summary>
    /// This is the standard FP32 test for CUDA-enabled devices.
    /// </summary>
    public sealed class CudaDeviceTestFp32 : ICudaDeviceTest
    {
        public CudaDeviceTestFp32(ICudaTestParameters parameters, IProgress<double> progress) : base(parameters, progress)
        {
            Initialize();
        }

        protected override CudaTestRecordCollection TestCsl(ICudaTestParameters parameters)
        {
            var generic_device = new CudaDevice(parameters.DeviceId, parameters.Range);
            using (var cuRand = new CuRand(generic_device))
            using (var cuStats = new CuStats(generic_device))
            using (var cuArray = new CuArray(generic_device))
            {
                var test = new List<CudaTestRecord>();

                // This will reduce the amount of array allocations we have to do.
                var results_cache = new float[parameters.Range];

                // Do the non-normal tests.
                var normal_dist_f32 = new CudaTestResultCuRand<float>(
                    nameof(cuRand.GenerateNormalDistribution),
                    x => cuRand.GenerateNormalDistribution(x, results_cache),
                    parameters.Range);
                test.Add(normal_dist_f32.TestRecord);

                var log_normal_dist_f32 = new CudaTestResultCuRand<float>(
                    nameof(cuRand.GenerateLogNormalDistribution),
                    i => cuRand.GenerateLogNormalDistribution(i, results_cache, 0, 1),
                    parameters.Range);
                test.Add(log_normal_dist_f32.TestRecord);

                var uniform_dist_f32 = new CudaTestResultCuRand<float>(
                    nameof(cuRand.GenerateUniformDistribution),
                    x => cuRand.GenerateUniformDistribution(x, results_cache),
                    parameters.Range);
                test.Add(uniform_dist_f32.TestRecord);

                // Do normal distribution.
                // The mean and standard deviation are known (mean = 0, standard deviation = 1).
                // We can valid the tests easily with normal distribution.

                // cache the results
                var cache_normal_results1 = new float[parameters.Range];
                var cache_normal_results2 = new float[parameters.Range];

                var normal_result1 = new CudaTestResultCuRand<float>(
                    nameof(cuRand.GenerateNormalDistribution),
                    x => cuRand.GenerateNormalDistribution(x, cache_normal_results1),
                    parameters.Range);
                test.Add(normal_result1.TestRecord);
                var std1 = new CudaTestResultcuStats<float, double>(
                    "|1 - std1| / 1 < 3%",
                    x => PercentageErrorCheck(1, x, 0.03),
                    cuStats.StandardDeviation,
                    cache_normal_results1);
                test.Add(std1.TestRecord);

                var normal_result2 = new CudaTestResultCuRand<float>(
                    nameof(cuRand.GenerateNormalDistribution),
                    x => cuRand.GenerateNormalDistribution(x, cache_normal_results2),
                    parameters.Range);
                test.Add(normal_result2.TestRecord);
                var std2 = new CudaTestResultcuStats<float, double>(
                    "|1 - std1| / 1 < 3%",
                    x => PercentageErrorCheck(1, x, 0.03),
                    cuStats.StandardDeviation,
                    cache_normal_results2);
                test.Add(std2.TestRecord);

                var cov = new CudaTestResultcuStats<float, double>(
                    "cov(1, 2) != 0",
                    x => x != 0,
                    cuStats.Covariance,
                    cache_normal_results1,
                    cache_normal_results2);
                var corr = new CudaTestResultcuStats<float, double>(
                    "|corr - cov(1, 2) / (std(1) * std(2))| < 1%",
                    x => PercentageErrorCheck(x, cov.Result / (std1.Result * std2.Result), 0.01),
                    cuStats.Correlation, cache_normal_results1, cache_normal_results2);

                var addition = new CudaTestResultcuStats<float, float[]>(
                    "addition[0] = result1[0] + result2[0]",
                    x => x[0] == cache_normal_results1[0] + cache_normal_results2[0],
                    cuArray.Add,
                    cache_normal_results1, cache_normal_results2);
                var subtraction = new CudaTestResultcuStats<float, float[]>(
                    "subtraction[0] = result1[0] - result2[0]",
                    x => x[0] == cache_normal_results1[0] - cache_normal_results2[0],
                    cuArray.Subtract,
                    cache_normal_results1, cache_normal_results2);

                test.Add(cov.TestRecord);
                test.Add(corr.TestRecord);
                test.Add(addition.TestRecord);
                test.Add(subtraction.TestRecord);

                var autocorrelation_lag0 = new CudaTestResultcuStats<float, double>(
                    "ACF, Lag 0, -1 <= x <= 1",
                    x => x >= -1 && x <= 1,
                    arr => cuStats.Autocorrelation(arr, 0),
                    cache_normal_results1);
                var autocorrelation_lag1 = new CudaTestResultcuStats<float, double>(
                    "ACF, Lag 1, -1 <= x <= 1",
                    x => x >= -1 && x <= 1,
                    arr => cuStats.Autocorrelation(arr, 1),
                    cache_normal_results1);
                var autocorrelation_lag2 = new CudaTestResultcuStats<float, double>(
                    "ACF, Lag 2, -1 <= x <= 1",
                    x => x >= -1 && x <= 1,
                    arr => cuStats.Autocorrelation(arr, 2),
                    cache_normal_results1);
                var autocorrelation_lag10 = new CudaTestResultcuStats<float, double>(
                    "ACF, Lag 10, -1 <= x <= 1",
                    x => x >= -1 && x <= 1,
                    arr => cuStats.Autocorrelation(arr, 10),
                    cache_normal_results1);
                var autocorrelation_lag1000 = new CudaTestResultcuStats<float, double>(
                    "ACF, Lag 1000, -1 <= x <= 1",
                    x => x >= -1 && x <= 1,
                    arr => cuStats.Autocorrelation(arr, 1000),
                    cache_normal_results1);

                test.Add(autocorrelation_lag0.TestRecord);
                test.Add(autocorrelation_lag1.TestRecord);
                test.Add(autocorrelation_lag2.TestRecord);
                test.Add(autocorrelation_lag10.TestRecord);
                test.Add(autocorrelation_lag1000.TestRecord);

                return new CudaTestRecordCollection(test, new GpuPerformanceMetrics(test.Sum(x => x.GpuElapsedMilliseconds)));
            }
        }
    }
}
