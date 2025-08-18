using DataProcessor.source.EngineWrapper.ComputationEngine;
using DataProcessor.source.ValueStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace test.TestEngine
{
   public class TestCalculationEngine
    {
        // Placeholder for future tests related to the Calculation Engine.
        // This class can be expanded with unit tests that validate the functionality of the Calculation Engine.
        // For example, tests can be added to check if calculations are performed correctly,
        // if exceptions are handled properly, or if the engine integrates well with other components.

        [Fact]
        public void TestCalculationEngine1()
        {
            var storage = new DataProcessor.source.ValueStorage.Int64ValuesStorage(new long?[] { 1, 2, 8, 4, 10 });
            Assert.Equal(25, CalculateSum.ComputeSum(storage.Cast<long>().ToArray()));

        }

        [Fact]
        public void TestCalculationWithNulls()
        {
            var storage = new DataProcessor.source.ValueStorage.Int64ValuesStorage(new long?[] { 1, null, 3, null, 5 });
            unsafe
            {
                long* ptr = (long*)storage.GetNativeBufferPointer();
                Span<long> span = new Span<long>(ptr, storage.Count);
                Assert.Equal(9, CalculateSum.ComputeSum(span.ToArray()));
            }
        }
    }
}
