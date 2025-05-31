using System.Runtime.CompilerServices;

namespace DataProcessor.source.Index
{
    internal static class IndexUtils
    {
        /// <summary>
        /// help methods to detect and handle multiindex
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool IsGroupedIndexElement(object? item)
        {
            return item is object[] ||
                   item is List<object> ||
                   item is ITuple || // covers both Tuple<> and ValueTuple<>
                   item?.GetType().FullName?.StartsWith("System.ValueTuple") == true;
        }

        // Kiểm tra cả danh sách index xem có phần tử nào là grouped index không
        public static bool ContainsGroupedIndex(IEnumerable<object?> index)
        {
            if (index == null) return false;
            foreach (var item in index)
            {
                if (IsGroupedIndexElement(item))
                    return true;
            }
            return false;
        }

        // Chuyển index về dạng flat index (flattened)
        public static IEnumerable<object> FlattenIndexElement(object? item)
        {
            if (item is object[] arr)
            {
                foreach (var el in arr) yield return el;
                yield break;
            }
              

            if (item is List<object> list)
            {
                foreach (var el in list) yield return el;
                yield break;
            }
                
            if (item is ITuple tuple)
            {
                for (int i = 0; i < tuple.Length; i++)
                    yield return tuple[i];
                yield break;
            }

            yield return item!;
        }

        // So sánh 2 index (gồm cả grouped index)
        public static bool IndexEquals(object? a, object? b)
        {
            if (IsGroupedIndexElement(a) && IsGroupedIndexElement(b))
            {
                var aList = new List<object>(FlattenIndexElement(a));
                var bList = new List<object>(FlattenIndexElement(b));

                if (aList.Count != bList.Count)
                    return false;

                for (int i = 0; i < aList.Count; i++)
                {
                    if (!Equals(aList[i], bList[i]))
                        return false;
                }

                return true;
            }

            return Equals(a, b);
        }

        // So sánh 2 index (gồm cả grouped index) với danh sách các index
        public class GroupedIndexEqualityComparer : IEqualityComparer<object?>
        {
            public new bool Equals(object? x, object? y)
            {
                return IndexUtils.IndexEquals(x, y);
            }

            public int GetHashCode(object? obj)
            {
                if (obj == null) return 0;

                if (IndexUtils.IsGroupedIndexElement(obj))
                {
                    unchecked
                    {
                        int hash = 17;
                        foreach (var element in IndexUtils.FlattenIndexElement(obj))
                        {
                            hash = hash * 23 + (element?.GetHashCode() ?? 0);
                        }
                        return hash;
                    }
                }

                return obj.GetHashCode();
            }
        }

    }
}
