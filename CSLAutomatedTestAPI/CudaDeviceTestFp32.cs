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

                // Do the non-normal tests.
                var normal_dist_f32 = new CudaTestResultCuRand<float>(
                    nameof(cuRand.GenerateNormalDistribution),
                    cuRand.GenerateNormalDistribution,
                    parameters.Range);
                var log_normal_dist_f32 = new CudaTestResultCuRand<float>(
                    nameof(cuRand.GenerateLogNormalDistribution),
                    i => cuRand.GenerateLogNormalDistribution(i, 0, 1),
                    parameters.Range);
                var uniform_dist_f32 = new CudaTestResultCuRand<float>(
                    nameof(cuRand.GenerateUniformDistribution),
                    cuRand.GenerateUniformDistribution,
                    parameters.Range);

                test.Add(normal_dist_f32.TestRecord);
                test.Add(log_normal_dist_f32.TestRecord);
                test.Add(uniform_dist_f32.TestRecord);

                // Do normal distribution.
                // The mean and standard deviation are known (mean = 0, standard deviation = 1).
                // We can valid the tests easily with normal distribution.

                var normal_result1 = new CudaTestResultCuRand<float>(
                    nameof(cuRand.GenerateNormalDistribution),
                    cuRand.GenerateNormalDistribution,
                    parameters.Range);
                var normal_result2 = new CudaTestResultCuRand<float>(
                    nameof(cuRand.GenerateNormalDistribution),
                    cuRand.GenerateNormalDistribution,
                    parameters.Range);

                test.Add(normal_result1.TestRecord);
                test.Add(normal_result2.TestRecord);

                //var std1 = generalTestOne("std1~=1", x => x > 0.97 && x < 1.03, cuStats.StandardDeviation, normal_result1.Result);
                var std1 = new CudaTestResultcuStats<float, double>(
                    "|1 - std1| / 1 < 3%",
                    x => PercentageErrorCheck(1, x, 0.03),
                    cuStats.StandardDeviation,
                    normal_result1.Result);
                var std2 = new CudaTestResultcuStats<float, double>(
                    "|1 - std1| / 1 < 3%",
                    x => PercentageErrorCheck(1, x, 0.03),
                    cuStats.StandardDeviation,
                    normal_result2.Result);

                test.Add(std1.TestRecord);
                test.Add(std2.TestRecord);

                var cov = new CudaTestResultcuStats<float, double>(
                    "cov(1, 2) != 0",
                    x => x != 0,
                    cuStats.Covariance,
                    normal_result1.Result,
                    normal_result2.Result);
                var corr = new CudaTestResultcuStats<float, double>(
                    "|corr - cov(1, 2) / (std(1) * std(2))| < 1%",
                    x => PercentageErrorCheck(x, cov.Result / (std1.Result * std2.Result), 0.01),
                    cuStats.Correlation, normal_result1.Result, normal_result2.Result);

                var addition = new CudaTestResultcuStats<float, float[]>(
                    "addition[0] = result1[0] + result2[0]",
                    x => x[0] == normal_result1.Result[0] + normal_result2.Result[0],
                    cuArray.Add,
                    normal_result1.Result, normal_result2.Result);
                var subtraction = new CudaTestResultcuStats<float, float[]>(
                    "subtraction[0] = result1[0] - result2[0]",
                    x => x[0] == normal_result1.Result[0] - normal_result2.Result[0],
                    cuArray.Subtract,
                    normal_result1.Result, normal_result2.Result);

                test.Add(cov.TestRecord);
                test.Add(corr.TestRecord);
                test.Add(addition.TestRecord);
                test.Add(subtraction.TestRecord);

                return new CudaTestRecordCollection(test, new GpuPerformanceMetrics(test.Sum(x => x.GpuElapsedMilliseconds)));
            }
        }
    }
}
