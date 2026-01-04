using DataProcessor.source.GenericsSeries;
using DataProcessor.source.UserSettings.DefaultValsGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test.TestGenericsSeries
{
    public class TestGroupView
    {
        private Series<int> CreateSampleSeries()
        {
            var values = new List<int> { 1, 2, 3, 4, 5, 6 };
            var index = new List<object> { "a", "b", "c", "d", "e", "f" };
            return new Series<int>(values, "numbers", index);
        }

        private Series<int>.GroupView CreateSampleGroupView(Series<int> series)
        {
            var groups = new Dictionary<object, int[]>
            {
                { "odd", new int[] { 0, 2, 4 } },   // 1, 3, 5
                { "even", new int[] { 1, 3, 5 } }   // 2, 4, 6
            };
            return new Series<int>.GroupView(series, groups);
        }

        [Fact]
        public void Count_ShouldReturnCorrectGroupSizes()
        {
            var series = CreateSampleSeries();
            var groupView = CreateSampleGroupView(series);

            var counts = groupView.Count();

            Assert.Equal(3u, counts["odd"]);
            Assert.Equal(3u, counts["even"]);
        }

        [Fact]
        public void Sum_ShouldComputeCorrectSums()
        {
            var series = CreateSampleSeries();
            var groupView = CreateSampleGroupView(series);

            var aggregator = new IntCalculator();
            var defaultGen = new IntDefaultValueGenerator();

            var sums = groupView.Sum(aggregator, defaultGen);

            Assert.Equal(9, sums["odd"]);  // 1+3+5
            Assert.Equal(12, sums["even"]); // 2+4+6
        }

        [Fact]
        public void Mean_ShouldComputeGroupMeans()
        {
            var series = CreateSampleSeries();
            var groupView = CreateSampleGroupView(series);

            var means = groupView.Mean(x => (double)x);

            Assert.Equal(3.0, means["odd"]);
            Assert.Equal(4.0, means["even"]);
        }

        [Fact]
        public void MinMax_ShouldReturnCorrectExtremes()
        {
            var series = CreateSampleSeries();
            var groupView = CreateSampleGroupView(series);

            var min = groupView.Min();
            var max = groupView.Max();

            Assert.Equal(1, min["odd"]);
            Assert.Equal(2, min["even"]);
            Assert.Equal(5, max["odd"]);
            Assert.Equal(6, max["even"]);
        }

        [Fact]
        public void Apply_ShouldTransformEachGroup()
        {
            var series = CreateSampleSeries();
            var groupView = CreateSampleGroupView(series);

            var result = groupView.Apply(s => s.Sum());

            Assert.Equal(9, result["odd"]);
            Assert.Equal(12, result["even"]);
        }

        [Fact]
        public void Transform_ShouldApplyElementwise()
        {
            var series = CreateSampleSeries();
            var groupView = CreateSampleGroupView(series);

            var transformed = groupView.Transform(x => x * 10);

            Assert.Equal(6, transformed.Count);
            Assert.Contains(10, transformed.Values);
            Assert.Contains(60, transformed.Values);
        }

        [Fact]
        public void Filter_ShouldKeepMatchingGroupsOnly()
        {
            var series = CreateSampleSeries();
            var groupView = CreateSampleGroupView(series);

            var filtered = groupView.Filter(s =>
            {
                var sum = s.Sum();
                return sum > 10;
            });

            Assert.Single(filtered.Groups); // only "even"
            Assert.True(filtered.Groups.ContainsKey("even"));
        }

        [Fact]
        public void Aggregate_ShouldRunMultipleAggregations()
        {
            var series = CreateSampleSeries();
            var groupView = CreateSampleGroupView(series);

            var agg = new Dictionary<string, Func<Series<int>, object>>
            {
                { "sum", s => s.Sum()},
                { "count", s => (uint)s.Count }
            };

            var result = groupView.Aggregate(agg);

            Assert.Equal(9, result["odd"]["sum"]);
            Assert.Equal((uint)3, result["odd"]["count"]);
        }

        [Fact]
        public void Accessors_FirstLastNth_ShouldReturnCorrectValues()
        {
            var series = CreateSampleSeries();
            var groupView = CreateSampleGroupView(series);

            Assert.Equal(1, groupView.First("odd"));
            Assert.Equal(5, groupView.Last("odd"));
            Assert.Equal(3, groupView.Nth("odd", 1));
        }

        [Fact]
        public void Accessors_ShouldThrowForInvalidKey()
        {
            var series = CreateSampleSeries();
            var groupView = CreateSampleGroupView(series);

            Assert.Throws<KeyNotFoundException>(() => groupView.First("unknown"));
            Assert.Throws<KeyNotFoundException>(() => groupView.Last("unknown"));
            Assert.Throws<IndexOutOfRangeException>(() => groupView.Nth("odd", 10));
        }

        // ----------------- Mock Calculator + Default Generator ----------------- //
        private class IntCalculator : ICalculator<int>
        {
            public int Add(int a, int b) => a + b;

            public int Divide(int a, int b)
            {
                throw new NotImplementedException();
            }

            public int Modulo(int a, int b)
            {
                throw new NotImplementedException();
            }

            public int Multiply(int a, int b)
            {
                throw new NotImplementedException();
            }

            public int Subtract(int a, int b)
            {
                throw new NotImplementedException();
            }
        }

        private class IntDefaultValueGenerator : IDefaultValueGenerator<int>
        {
            public int GenerateDefaultValue() => 0;
        }
    }
}
