using DataProcessor.source.UserSettings.DefaultValsGenerator;
using DataProcessor.source.ValueStorage;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DataProcessor.source.EngineWrapper.ComputationEngine
{
    /// <summary>
    /// this class provides methods to compute the sum of arrays of doubles and longs using AVX instructions.
    /// <remarks> currently this is builded for <see cref="Int64ValuesStorage"/> <see cref="DoubleValueStorage"/> for other sum methods / other calculation 
    /// methods delevelopers see <see cref="FuncCalculator{T}"/> and <see cref="FuncDefaultValueGenerator{T}"/></remarks>
    /// </summary>
    internal static class CalculateSum
    {

        /// <summary>
        /// This method uses SIMD to calculate sum of double array
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="PlatformNotSupportedException"></exception>
        public static double ComputeSum(double[] data)
        {
            if (!Avx.IsSupported)
                throw new PlatformNotSupportedException("AVX is not supported on this CPU");
            double sum = 0.0;
            int length = data.Length;
            int i = 0;
            // Process 4 doubles at a time using AVX
            if (length >= 4)
            {
                Vector256<double> sumVector = Vector256<double>.Zero;
                for (; i <= length - 4; i += 4)
                {
                    // Use MemoryMarshal to get a pointer to the data
                    unsafe
                    {
                        fixed (double* ptr = &data[i])
                        {
                            Vector256<double> dataVector = Avx.LoadVector256(ptr);
                            sumVector = Avx.Add(sumVector, dataVector);
                        }
                    }
                }
                // Horizontal add to get the sum of the vector
                Vector128<double> lower = sumVector.GetLower();
                Vector128<double> upper = sumVector.GetUpper();
                Vector128<double> horizontalSum = Sse2.Add(lower, upper);
                sum += horizontalSum.ToScalar();
            }
            // Process remaining elements
            for (; i < length; i++)
            {
                sum += data[i];
            }

            return sum;
        }

        /// <summary>
        /// this method uses SIMD to calculate sum of Int64 array
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="PlatformNotSupportedException"></exception>
        public static long ComputeSum(long[] data)
        {
            if (!Avx.IsSupported)
                throw new PlatformNotSupportedException("AVX is not supported on this CPU");
            long sum = 0L;
            int length = data.Length;
            int i = 0;
            // Process 4 longs at a time using AVX
            if (length >= 4)
            {
                Vector256<long> sumVector = Vector256<long>.Zero;
                for (; i <= length - 4; i += 4)
                {
                    // Use MemoryMarshal to get a pointer to the data
                    unsafe
                    {
                        fixed (long* ptr = &data[i])
                        {
                            Vector256<long> dataVector = Avx.LoadVector256(ptr);
                            sumVector = Avx2.Add(sumVector, dataVector);
                        }
                    }
                }
                // Horizontal add to get the sum of the vector
                Vector128<long> lower = sumVector.GetLower();
                Vector128<long> upper = sumVector.GetUpper();
                Vector128<long> horizontalSum = Sse2.Add(lower, upper);
                Span<long> buf = stackalloc long[2];
                unsafe
                {
                    fixed (long* pBuf = buf)
                    {
                        Sse2.Store(pBuf, horizontalSum);
                    }
                }
                sum += buf[0] + buf[1];
            }
            // Process remaining elements
            for (; i < length; i++)
            {
                sum += data[i];
            }
            return sum;
        }

        /// <summary>
        /// this method uses SIMD to calculate sum of Int32 array
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="PlatformNotSupportedException"></exception>
        public static long ComputeSum(int[] data)
        {
            if (!Avx.IsSupported)
                throw new PlatformNotSupportedException("AVX is not supported on this CPU");
            long sum = 0L;
            int length = data.Length;
            int i = 0;
            // Process 4 longs at a time using AVX
            if (length >= 4)
            {
                Vector256<int> sumVector = Vector256<int>.Zero;
                for (; i <= length - 4; i += 4)
                {
                    // Use MemoryMarshal to get a pointer to the data
                    unsafe
                    {
                        fixed (int* ptr = &data[i])
                        {
                            Vector256<int> dataVector = Avx.LoadVector256(ptr);
                            sumVector = Avx2.Add(sumVector, dataVector);
                        }
                    }
                }
                // Horizontal add to get the sum of the vector
                Vector128<int> lower = sumVector.GetLower();
                Vector128<int> upper = sumVector.GetUpper();
                Vector128<int> horizontalSum = Sse2.Add(lower, upper);
                Span<int> buf = stackalloc int[2];
                unsafe
                {
                    fixed (int* pBuf = buf)
                    {
                        Sse2.Store(pBuf, horizontalSum);
                    }
                }
                sum += buf[0] + buf[1];
            }
            // Process remaining elements
            for (; i < length; i++)
            {
                sum += data[i];
            }
            return sum;
        }

        /// <summary>
        /// this method calculate sum of specified type which can be add
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="dropNull"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static T ComputeSum<T>(T[] data, bool dropNull = true)
        {
            // data must not be null
            if(data == null)
            {
                throw new ArgumentNullException(nameof(data), "Data cannot be null.");
            }
            // check if the type is supported
            var calculator =  AggregationRegistry.GetAggregator<T>();
            var defaultProvider = AggregationRegistry.GetDefaultValueProvider<T>();
            var sum = defaultProvider.GenerateDefaultValue();

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] is null && dropNull)
                {
                    continue;
                }
                if (data[i] is not T value)
                {
                    throw new ArgumentException($"Unsupported type for computation: {typeof(T)}");
                }
                sum =  calculator.Add(value, sum);
            }
            return sum;
        }
    }
}
