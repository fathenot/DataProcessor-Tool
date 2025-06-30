using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataProcessor.source.NonGenericsSeries;
using DataProcessor.source.ValueStorage;
namespace test
{
    public class TestMemory
    {
        [Fact]
        public void Series_ShouldNot_Leak_Memory()
        {
            long before = GC.GetTotalMemory(true);

            void CreateAndDispose()
            {
                var data = Enumerable.Range(0, 100_000).Select(x => (long?)x).ToArray();

                using var storage = new IntValuesStorage(data); // <= quan trọng

                // Nếu bạn không dùng `using`, thì cần gọi `.Dispose()` bằng tay
            }

            CreateAndDispose();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect(); // vòng 2 để chắc

            long after = GC.GetTotalMemory(true);

            Assert.True(after - before < 2* 1024*1024);
        }

    }
}
