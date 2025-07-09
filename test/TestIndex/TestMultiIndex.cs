using DataProcessor.source.Index;

namespace DataProcessor.Tests.Index
{
    public class MultiIndexTests
    {

        [Fact]
        public void MultiKey_ShouldInitializeCorrectly()
        {
            var key = new MultiKey("A", 1);
            Assert.Equal("A", key[0]);
            Assert.Equal(1, key[1]);
            Assert.Equal(2, key.Length);
        }
        [Fact]
        public void MultiKey_ShouldBeEqual()
        {
            var key1 = new MultiKey("A", 1);
            var key2 = new MultiKey("A", 1);
            var key3 = new MultiKey("B", 2);
            Assert.Equal(key1, key2);
            Assert.NotEqual(key1, key3);
            Assert.True(key1.Equals(key2));
            Assert.False(key1.Equals(key3));
        }
        [Fact]
        public void MultiKey_ShouldBeHashable()
        {
            var key1 = new MultiKey("A", 1);
            var key2 = new MultiKey("A", 1);
            var key3 = new MultiKey("B", 2);
            Assert.Equal(key1.GetHashCode(), key2.GetHashCode());
            Assert.NotEqual(key1.GetHashCode(), key3.GetHashCode());
        }

        [Fact]
        public void FilterMultikey_From_List_ofObject()
        {
            var data = new List<object>
            {
                new object[] {"A", 1},
                new object[] {"B", 2},
                new object[] {"A", 34} // Duplicate to test multiple positions
            };

            MultiKey[] keys = data.Select(k =>
            {
                if (k is object[] arr)
                    return new MultiKey(arr);
                else
                    return new MultiKey(k);
            }).ToArray();
            Assert.Equal(3, keys.Length);
            foreach(var key in keys)
            {
                Assert.IsType<MultiKey>(key);
                Assert.Equal(2, key.Length);
            }

            var selectedKeys = new List<object>
            {
                new object[] {"A", 1},
                new object[] {"A", 34} // Duplicate to test multiple positions
            };
            var convertedToMultikeys = selectedKeys.Select(k =>
            {
                if (k is object[] arr)
                    return new MultiKey(arr);
                else
                    return new MultiKey(k);
            }).ToList();
            var filteredKeys = keys.Where(item => convertedToMultikeys.Contains(new MultiKey(item)))
                .ToList();
            Assert.Equal(2, filteredKeys.Count);
        }

        [Fact]
        public void Constructor_ShouldInitializeCorrectly()
        {
            var data = new List<object[]>
            {
                new object[] {"A", 1},
                new object[] {"B", 2},
                new object[] {"A", 1} // Duplicate to test multiple positions
            };
            Assert.Equal(3, data.Count);
            var multiIndex = new MultiIndex(data);

            Assert.Equal(3, multiIndex.Count);
            Assert.True(multiIndex.Contains(new object[] { "A", 1 }));
            Assert.True(multiIndex.Contains(new MultiKey("B", 2)));
            //Assert.Equal(new List<int> { 0, 2 }, multiIndex.GetIndexPosition(new object[] { "A", 1 }));
        }

        [Fact]
        public void SliceLevel_ShouldFilterCorrectly()
        {
            var data = new List<object[]>
            {
                new object[] {"X", 10},
                new object[] {"Y", 20},
                new object[] {"X", 30}
            };
            var multiIndex = new MultiIndex(data);
            var sliced = multiIndex.SliceLevel(0, "X");

            Assert.Equal(2, sliced.Count);
            Assert.True(sliced.Contains(new object[] { "X", 10 }));
            Assert.True(sliced.Contains(new object[] { "X", 30 }));
        }

        [Fact]
        public void GetIndex_ShouldReturnCorrectMultiKey()
        {
            var data = new List<object[]>
            {
                new object[] {"A", 1},
                new object[] {"B", 2}
            };
            var multiIndex = new MultiIndex(data);

            var key = multiIndex.GetIndex(1);

            Assert.IsType<MultiKey>(key);
            Assert.Equal(new MultiKey("B", 2), key);
        }

        [Fact]
        public void IndexerSet_ShouldUpdateCorrectly()
        {
            var data = new List<object[]>
            {
                new object[] {"A", 1},
                new object[] {"B", 2}
            };
            var multiIndex = new MultiIndex(data);
            var newKey = new MultiKey("C", 3);

            Assert.True(multiIndex.Contains(new object[] { "A", 1 }));
            Assert.True(multiIndex.Contains(new object[] { "B", 2 }));
            Assert.Equal(new List<int> { 1 }, multiIndex.GetIndexPosition(new object[] { "B", 2 }));
        }

        [Fact]
        public void DistinctIndices_ShouldReturnDistinctElements()
        {
            var data = new List<object[]>
            {
                new object[] {"A", 1},
                new object[] {"B", 2},
                new object[] {"A", 1}
            };
            var multiIndex = new MultiIndex(data);

            var distinct = new HashSet<object>(multiIndex.DistinctIndices());

            Assert.Equal(2, distinct.Count);
        }

        [Fact]
        public void TestSearchingFirstPosition()
        {
            var data = new List<object[]>
            {
                new object[] {"A", 1},
                new object[] {"B", 2},
                new object[] {"A", 1}
            };

            var multiIndex = new MultiIndex(data);
            Assert.Equal(0, multiIndex.FirstPositionOf(new object[] { "A", 1 }));
        }

        [Fact]
        public void TestSlice_ShouldReturnCorrectSubIndex()
        {
            var data = new List<object[]>
            {
                new object[] {"A", 1},
                new object[] {"B", 2},
                new object[] {"C", 3}
            };
            var multiIndex = new MultiIndex(data);
            var sliced = multiIndex.Slice(0, 2);
            Assert.Equal(2, sliced.Count);
            Assert.True(sliced.Contains(new object[] { "A", 1 }));
            Assert.True(sliced.Contains(new object[] { "B", 2 }));
        }

        [Fact]
        public void TestSliceWithStep_ShouldReturnCorrectSubIndex()
        {
            var data = new List<object[]>
            {
                new object[] {"A", 1},
                new object[] {"B", 2},
                new object[] {"C", 3},
                new object[] {"D", 4}
            };
            var multiIndex = new MultiIndex(data);
            var sliced = multiIndex.Slice(0, 4, 2);
            Assert.Equal(2, sliced.Count);
            Assert.True(sliced.Contains(new object[] { "A", 1 }));
            Assert.True(sliced.Contains(new object[] { "C", 3 }));
        }

        [Fact]
        public void TestSliceIndex()
        {
            var data = new List<object[]>
            {
                new object[] {"A", 1},
                new object[] {"B", 2},
                new object[] {"A", 3}
            };
            var multiIndex = new MultiIndex(data);
            var sliced = multiIndex.Slice(new List<object>
            {
                (new object[] { "A", 1 })
            });
            Assert.Equal(1, sliced.Count);
            Assert.True(sliced.Contains(new object[] { "A", 1 }));
        }

        [Fact]
        public void TestSliceWithMultipleKeys_ShouldThrowExxceptionWithNonExistingKey()
        {
            var data = new List<object[]>
            {
                new object[] {"A", 1},
                new object[] {"B", 2},
                new object[] {"A", 3},
                new object[] {"C", 4}
            };
            var multiIndex = new MultiIndex(data);
            Assert.Throws<ArgumentException>(()=>multiIndex.Slice(new List<object> { "A", "B" }));
        }

        [Fact]
        public void TestContains_ShouldReturnTrueForExistingKey()
        {
            var data = new List<object[]>
            {
                new object[] {"A", 1},
                new object[] {"B", 2}
            };
            var multiIndex = new MultiIndex(data);
            Assert.True(multiIndex.Contains(new object[] { "A", 1 }));
            Assert.False(multiIndex.Contains(new object[] { "C", 3 }));
        }

        [Fact]
        public void ApplyLinq()
        {

        }
    }
}
