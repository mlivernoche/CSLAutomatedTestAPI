CSL Automated Test API

This API is used can be used to test the CudaSharperLibrary (CSL) on CUDA-enabled GPUs. This API automates these tests and can be
used to build a console or GUI application.

Objectives:
1. Use all of the functionality provided by the CSL.
All functions defined in CSL should be tested.

2. Validate the error codes that returned by each function.
All functions defined in CSL should return cudaSucess error code.

3. Validate the results returned on a case-by-case basis.
Validation should be defined by the test API, and should be for specific
functions. For example, a function generating a normal random distribution
should be checked for repeating digits (this can happen due to a cudaErrorLaunchFailure
or the CUDA kernel did not hit every part of the array). These can be expanded upon
as unintended behaviors are found.

4. These tests should be automated.
These tests should be automated, except in the case of defining the parameters of the test.
Defining the test parameters (e.g., how intense the tests should be) can be done in the command
line or in a GUI or any other method.